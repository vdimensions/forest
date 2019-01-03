namespace Forest.Web.WebSharper

open Forest
open WebSharper
open WebSharper.UI

type ClientNode<'T> =
    {
        Hash: thash;
        Name: vname;
        Model: 'T;
        Regions: Map<rname, thash list>
    }

type [<Interface>] IDocProvider =
    abstract member Doc: unit -> Doc
    abstract member DocView: unit -> View<Doc>
    abstract member DocVar: unit -> Var<Doc>

type [<AbstractClass>] WebSharperPhysicalView private (nodeVar : Var<DomNode>) =
    new (node : DomNode) = WebSharperPhysicalView(Var.Create node)
    abstract member Doc: Map<rname, IDocProvider list> -> Doc
    member internal __.NodeVar with get() = nodeVar

type [<AbstractClass>] WebSharperDocumentView<'M>(node) =
    inherit WebSharperPhysicalView(node)
    abstract member Render: model : 'M  -> rdata : Map<rname, IDocProvider list> -> Doc
    override this.Doc rdata =
        let model = (node.Model :?> 'M)
        this.Render model rdata
    abstract member ToDoc: ClientNode<'M> -> Doc
    [<JavaScript>]
    member this.ToClientDoc node = this.ToDoc node
    
type [<AbstractClass>] WebSharperTemplateView<'M, 'T>(node) =
    inherit WebSharperDocumentView<'M>(node)
    abstract member InstantiateTemplate: unit -> 'T
    abstract member RenderTemplate: model : 'M -> template : 'T -> rdata : Map<rname, IDocProvider list> -> Doc
    override this.Render model rdata =
        let template = this.InstantiateTemplate()
        this.RenderTemplate model template rdata
