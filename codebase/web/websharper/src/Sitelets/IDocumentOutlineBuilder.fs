namespace Forest.Web.WebSharper.Sitelets
open WebSharper
open WebSharper.UI

type AfterRenderCallback = Quotations.Expr<(JavaScript.Dom.Element -> unit)>

type [<Interface>] IDocumentOutlineBuilder =
    abstract member AddHeader: header : Doc -> IDocumentOutlineBuilder
    abstract member AddBody: body : Doc -> IDocumentOutlineBuilder
    abstract member AddAfterRender: callback : AfterRenderCallback -> IDocumentOutlineBuilder