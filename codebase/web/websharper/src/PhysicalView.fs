namespace Forest.Web.WebSharper

open Forest
open WebSharper
open WebSharper.UI

[<JavaScriptExport>]
type [<AbstractClass;NoEquality;NoComparison>] WebSharperPhysicalView() =
    [<JavaScriptExport>]
    abstract member Doc: array<rname*Doc> -> Node -> Doc

[<JavaScript>]
module ClientCode =
    open WebSharper.UI.Client
    open WebSharper.UI.Html

    let private _nodes = List.empty |> ListModel.Create<obj,Node> (fun n -> n.Model)
    let private _views : Var<Map<vname, WebSharperPhysicalView>> = Var.Create <| Map.empty

    let setNodes nodes = nodes |> _nodes.Set
    let setViews nodes = nodes |> _views.Set

    let private syncNodes(force) : unit =
        if (force || _nodes.Length = 0) then
            async {
                let! n = Remoting.GetNodes()
                n |> setNodes
            }
            |> Async.StartImmediate

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
        client <@ tree() |> Doc.BindView (fun t -> traverseTree t hash) @>

    let render () =
        client <@ tree().Map treeRooted |> Doc.BindView (fun (r, t) -> r |> Seq.map (traverseTree t) |> Doc.Concat); @>

    let internal executeCommand hash cmd (arg : obj) =
        async {
            let! _ = Remoting.ExecuteCommand hash cmd arg            
            syncNodes(true)
        }
        |> Async.Start

[<JavaScriptExport>]
type [<AbstractClass;NoEquality;NoComparison>] WebSharperPhysicalView<'M>() =
    inherit WebSharperPhysicalView()
    member __.ExecuteCommand hash name arg = ClientCode.executeCommand hash name arg

[<JavaScriptExport>]
type [<AbstractClass;NoEquality;NoComparison>] WebSharperDocumentView<'M>() =
    inherit WebSharperPhysicalView<'M>()
    [<JavaScriptExport>]
    abstract member Render: hash : thash -> model : 'M  -> rdata : array<rname*Doc> -> Doc
    
    [<JavaScriptExport>]
    override this.Doc rdata node =
        let model = (node.Model :?> 'M)
        this.Render node.Hash model rdata
    
[<JavaScriptExport>]
type [<AbstractClass>] WebSharperTemplateView<'M, 'T>() =
    inherit WebSharperDocumentView<'M>()
    abstract member InstantiateTemplate: unit -> 'T
    abstract member RenderTemplate: hash : thash -> model : 'M -> template : 'T -> rdata : array<rname*Doc> -> Doc
    override this.Render hash model rdata =
        let template = this.InstantiateTemplate()
        this.RenderTemplate hash model template rdata
