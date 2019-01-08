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
        if (force || _nodes.Value |> Seq.isEmpty) then
            async {
                let! n = Remoting.GetNodes()
                n |> _nodes.Set
            }
            |> Async.StartImmediate

    let afterRender (_ : JavaScript.Dom.Element) = 
        syncNodes(false)

    // --------------------------------------
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
        
    let private directDoc (docView : View<Doc>) = 
        docView |> Doc.EmbedView
    let inline private clientDoc (docView : View<Doc>) : Doc =
        client <@ docView |> directDoc @>

    let rec private processDocInternal (t : Map<thash,Node*WebSharperPhysicalView>) (hash : thash) : Doc =
        match t |> Map.tryFind hash with
        | None -> Doc.Empty
        | Some (node, pv) ->
            let regionDocs = 
                node.Regions 
                |> Array.map (fun (rname, v) -> rname, v |> Array.map (fun x -> processDocInternal t x) |> Doc.Concat)
            pv.Doc regionDocs node
    let private processDoc (wrapper : View<Doc> -> Doc) (tree : View<Map<thash,Node*WebSharperPhysicalView>>) (hash : thash) : Doc =
        tree |> View.Map (fun t -> processDocInternal t hash) |> wrapper
    let render hash =
        client <@ syncNodes(false); tree() |> Doc.BindView (fun t -> processDocInternal t hash) @>

    let renderRegionsOnServer (rdata : array<rname * thash array>) = 
        rdata |> Array.map (fun (r, hs) -> r, hs |> Array.map (fun h -> processDoc clientDoc (tree()) h) |> Doc.Concat)
    // --------------------------------------

    let internal executeCommand hash cmd (arg : obj) =
        Remoting.ExecuteCommand hash cmd arg
        syncNodes(true)

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
