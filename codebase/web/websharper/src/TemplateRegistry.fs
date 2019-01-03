namespace Forest.Web.WebSharper
open Forest

type WebSharperPhysicalViewFactory = INode -> WebSharperPhysicalView

type [<Interface>] IWebSharperTemplateRegistry =
    abstract member Register: name : vname -> factory : WebSharperPhysicalViewFactory -> IWebSharperTemplateRegistry
    abstract member Get: domNode : INode -> WebSharperPhysicalView