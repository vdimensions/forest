namespace Forest

open Forest.Reflection


type [<AbstractClass>] AbstractForestContext (viewFactory:IViewFactory, reflectionProvider:IReflectionProvider, securityManager:ISecurityManager) =
    let _viewRegistry = DefaultViewRegistry(viewFactory, reflectionProvider)
    member __.ViewRegistry with get() : IViewRegistry = upcast _viewRegistry
    member __.SecurityManager with get() : ISecurityManager = securityManager
    interface IForestContext with
        member this.ViewRegistry = this.ViewRegistry
        member this.SecurityManager = this.SecurityManager

type DefaultForestContext(viewFactory:IViewFactory, reflectionProvider:IReflectionProvider, securityManager:ISecurityManager) =
    inherit AbstractForestContext(viewFactory, reflectionProvider, securityManager)
    new (viewFactory:IViewFactory, securityManager:ISecurityManager) = DefaultForestContext(viewFactory, DefaultReflectionProvider(), securityManager)
