namespace Forest.Web.WebSharper
open Forest

type WebSharperPhysicalViewFactory = DomNode -> WebSharperPhysicalView

type [<Interface>] IWebSharperTemplateRegistry =
    abstract member Register: name : vname -> factory : WebSharperPhysicalViewFactory -> IWebSharperTemplateRegistry
    abstract member Get: domNode : DomNode -> WebSharperPhysicalView