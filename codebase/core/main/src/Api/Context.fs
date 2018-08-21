namespace Forest

open System


type [<Interface>] IForestContext =
    abstract ViewRegistry: IViewRegistry with get
    // TODO: renderers
    abstract SecurityManager: ISecurityManager with get