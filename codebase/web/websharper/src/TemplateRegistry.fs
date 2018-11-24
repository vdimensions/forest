namespace Forest.Web.WebSharper
open Forest
open Forest.UI

type WebSharperPhysicalViewFactory = ICommandDispatcher*DomNode -> WebSharperPhysicalView

type [<Interface>] IWebSharperTemplateRegistry =
    abstract member Register: name : vname -> factory : WebSharperPhysicalViewFactory -> IWebSharperTemplateRegistry
    abstract member Get: commandDispatcher : ICommandDispatcher -> domNode : DomNode -> WebSharperPhysicalView