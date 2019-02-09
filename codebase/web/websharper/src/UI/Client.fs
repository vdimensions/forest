namespace Forest.Web.WebSharper.UI
open System
open Forest
open Forest.Web.WebSharper
open WebSharper
open WebSharper.UI


[<JavaScriptExport>]
type [<AbstractClass;NoEquality;NoComparison>] WebSharperPhysicalView() =

    [<JavaScriptExport>]
    abstract member Doc: array<rname*Doc> -> Node -> Doc

    [<JavaScript(false)>]
    member this.GetClientTypeName() =
        this.GetType().FullName.Replace("+", ".")

    [<JavaScript(false)>]
    abstract member GetLogicalViewType: unit -> Type

[<JavaScript>]
module Client =
    open WebSharper.UI.Client

    [<NoEquality;NoComparison>]
    type Engine =
        {
            executeRawCommand : (cname -> obj -> unit);
            executeRpcCommand : ((thash -> Async<Node array>) -> unit);
            renderRegion : (rname -> Doc);
        }

    let private _nodes = List.empty |> ListModel.Create<obj,Node> (fun n -> n.Model)
    let private _views : Var<Map<vname, WebSharperPhysicalView>> = Var.Create <| Map.empty

    let setNodes nodes = nodes |> _nodes.Set
    let setViews nodes = nodes |> _views.Set
    let registerView name pv =
        _views.Value
        |> Map.add name pv
        |> setViews

    let private syncNodes(force) : unit =
        if (force || _nodes.Length = 0) then
            async {
                let! n = Remoting.GetNodes()
                n |> setNodes
            }
            |> Async.StartImmediate

    let internal emptyDoc () =
        Doc.Empty

    let init () = 
        syncNodes(false)

    let private tree () : View<Map<thash,Node*WebSharperPhysicalView>> =
        View.Map2
            (fun (a : Node seq) b -> 
                a 
                |> Seq.map (fun (a) -> a.Hash, a, (b |> Map.tryFind a.Name))
                |> Seq.map (fun (a, b, c) -> match c with | None -> None | Some c1 -> Some (a, (b, c1)))
                |> Seq.choose id
                |> Map.ofSeq
            )
            (_nodes.View)
            (_views.View)

    let private treeRooted (t : Map<thash,Node*WebSharperPhysicalView>) =
        let mutable rootKeys = t |> Seq.map (fun x -> x.Key) |> Set.ofSeq
        for (n, _) in t |> Seq.map (fun x -> x.Value) do
            for (_, rs) in n.Regions do
                for h in rs do
                    rootKeys <- rootKeys |> Set.remove h
        rootKeys |> Set.toArray, t
        
    let rec private traverseTree (t : Map<thash,Node*WebSharperPhysicalView>) (hash : thash) : Doc =
        match t |> Map.tryFind hash with
        | None -> Doc.Empty
        | Some (node, pv) ->
            let regionDocs = 
                node.Regions 
                |> Array.map (fun (rname, v) -> rname, v |> Array.map (fun x -> traverseTree t x) |> Doc.Concat)
            pv.Doc regionDocs node

    let renderNode hash =
        tree() |> Doc.BindView (fun t -> traverseTree t hash)

    let render () =
        tree().Map treeRooted |> Doc.BindView (fun (r, t) -> r |> Seq.map (traverseTree t) |> Doc.Concat)

    let internal executeRpcCommand (rpc : (thash -> Async<Node array>)) hash =
        async {
            let! nodes = rpc hash            
            setNodes nodes
        }
        |> Async.Start
    let internal executeRawCommand cmd hash (arg : obj) = executeRpcCommand (fun h -> Remoting.ExecuteCommand cmd h arg) hash

    [<JavaScriptExport>]
    type ViewRegistry internal(name : vname) =
        member __.Register(view : WebSharperPhysicalView) = registerView name view

[<JavaScriptExport>]
type [<AbstractClass;NoEquality;NoComparison>] WebSharperPhysicalView<'V, 'M when 'V :> IView<'M>>() =
    inherit WebSharperPhysicalView()

    [<JavaScript(false)>]
    override __.GetLogicalViewType() = typeof<'V>

[<JavaScriptExport>]
type [<AbstractClass;NoEquality;NoComparison>] WebSharperDocumentView<'V, 'M when 'V :> IView<'M>>() =
    inherit WebSharperPhysicalView<'V, 'M>()

    [<JavaScriptExport>]
    abstract member Render: model : 'M  -> client : Client.Engine -> Doc list
    
    [<JavaScriptExport>]
    override this.Doc rdata node =
        let rdataArr, hash = rdata |> Map.ofArray, node.Hash

        let rr = rdataArr.TryFind >> Option.defaultWith Client.emptyDoc
        let execrawcmd cmd (arg : obj) = Client.executeRawCommand cmd hash arg
        let execrpccmd rpc = Client.executeRpcCommand rpc hash 

        let client : Client.Engine = { renderRegion = rr; executeRawCommand = execrawcmd; executeRpcCommand = execrpccmd}
        let model = (node.Model :?> 'M)

        this.Render model client |> Doc.Concat
    
[<JavaScriptExport>]
type [<AbstractClass>] WebSharperTemplateView<'V, 'M, 'T when 'V :> IView<'M>>() =
    inherit WebSharperDocumentView<'V, 'M>()
    abstract member InstantiateTemplate: unit -> 'T
    abstract member RenderTemplate: model : 'M -> template : 'T -> client : Client.Engine -> Doc
    override this.Render model client =
        let template = this.InstantiateTemplate()
        [this.RenderTemplate model template client]
