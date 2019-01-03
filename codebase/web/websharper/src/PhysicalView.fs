namespace Forest.Web.WebSharper

open Forest
open WebSharper
open WebSharper.UI


type [<Interface>] IDocProvider =
    abstract member Doc: unit -> Doc
    abstract member DocView: unit -> View<Doc>
    abstract member DocVar: unit -> Var<Doc>

type [<AbstractClass;JavaScriptExport>] WebSharperPhysicalView() =
    abstract member Doc: Map<rname, IDocProvider list> -> Doc

type [<AbstractClass>] WebSharperDocumentView<'M>(node : INode) =
    inherit WebSharperPhysicalView()
    [<JavaScriptExport>]
    abstract member Render: model : 'M  -> rdata : Map<rname, IDocProvider list> -> Doc
    override this.Doc rdata =
        let model = (node.Model :?> 'M)
        this.Render model rdata
    [<JavaScriptExport>]
    member this.ToDoc (node: INode): Doc =
        let model = (node.Model :?> 'M)
        this.Render model Map.empty
    [<JavaScript>]
    member this.ToClientDoc (node: INode) = this.ToDoc node
    
type [<AbstractClass>] WebSharperTemplateView<'M, 'T>(node : INode) =
    inherit WebSharperDocumentView<'M>(node)
    abstract member InstantiateTemplate: unit -> 'T
    abstract member RenderTemplate: model : 'M -> template : 'T -> rdata : Map<rname, IDocProvider list> -> Doc
    override this.Render model rdata =
        let template = this.InstantiateTemplate()
        this.RenderTemplate model template rdata
