namespace Forest.Web.WebSharper.Sitelets
open Forest
open Forest.Web.WebSharper.UI


type [<Interface>] IWebSharperTemplateRegistry =
    abstract member Register: name : vname -> physicalViewExpr : WebSharperPhysicalView -> IWebSharperTemplateRegistry

module TemplateRegistry =
    let register (name : vname) (physicalView : WebSharperPhysicalView) (registry : IWebSharperTemplateRegistry) =
        registry.Register name physicalView
