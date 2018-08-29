namespace Forest


type [<Interface>] IForestContext =
    abstract ViewRegistry:IViewRegistry with get
    abstract SecurityManager:ISecurityManager with get
    // TODO: renderers