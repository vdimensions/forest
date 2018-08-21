namespace Forest


type [<AbstractClass>] AbstractForestContext (viewRegistry: IViewRegistry, securityManager: ISecurityManager) as self =
    new (viewFactory: IViewFactory, securityManager: ISecurityManager) = AbstractForestContext(DefaultViewRegistry(viewFactory), securityManager)
    member __.ViewRegistry with get(): IViewRegistry = viewRegistry
    member __.SecurityManager with get(): ISecurityManager = securityManager
    interface IForestContext with
        member __.ViewRegistry = self.ViewRegistry
        member __.SecurityManager = self.SecurityManager

type DefaultForestContext(viewFactory: IViewFactory, securityManager: ISecurityManager) =
    inherit AbstractForestContext(viewFactory, securityManager)
