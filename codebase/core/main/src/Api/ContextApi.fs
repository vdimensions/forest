namespace Forest

open Forest.Security
open Forest.Templates.Raw


type [<Interface>] IForestContext =
    abstract ViewRegistry:IViewRegistry with get
    abstract SecurityManager:ISecurityManager with get
    abstract TemplateProvider:ITemplateProvider with get
    // TODO: renderers