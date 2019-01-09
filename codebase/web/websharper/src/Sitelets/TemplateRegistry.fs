namespace Forest.Web.WebSharper.Sitelets
open Forest
open Forest.Web.WebSharper


type [<Interface>] IWebSharperTemplateRegistry =
    abstract member Register: name : vname -> physicalViewExpr : Quotations.Expr<WebSharperPhysicalView> -> IWebSharperTemplateRegistry

module TemplateRegistry =
    let register (name : vname) (physicalViewExpr : Quotations.Expr<WebSharperPhysicalView>) (registry : IWebSharperTemplateRegistry) =
        registry.Register name physicalViewExpr
