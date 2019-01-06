namespace Forest.Web.WebSharper
open Forest

type WebSharperPhysicalViewFactory = unit -> WebSharperPhysicalView

type [<Interface>] IWebSharperTemplateRegistry =
    abstract member Register: name : vname -> factory : WebSharperPhysicalView -> IWebSharperTemplateRegistry
    abstract member Get: name : vname -> WebSharperPhysicalView