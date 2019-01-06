namespace Forest.Web.WebSharper

open Forest
open WebSharper
open WebSharper.UI


[<JavaScriptExport>]
type [<AbstractClass>] WebSharperPhysicalView() =
    [<JavaScriptExport>]
    abstract member Doc: array<rname*Doc> -> Node -> Doc

[<JavaScriptExport>]
type [<AbstractClass>] WebSharperDocumentView<'M>() =
    inherit WebSharperPhysicalView()
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
