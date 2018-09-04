namespace Forest


type [<AbstractClass>] AbstractForestContext (viewRegistry:IViewRegistry, securityManager:ISecurityManager) =
    member __.ViewRegistry with get() : IViewRegistry = viewRegistry
    member __.SecurityManager with get() : ISecurityManager = securityManager
    interface IForestContext with
        member this.ViewRegistry = this.ViewRegistry
        member this.SecurityManager = this.SecurityManager

type NoopSecurityManager() =
    interface ISecurityManager with
        member __.HasAccess(_:ICommandDescriptor) = true
        member __.HasAccess(_:IViewDescriptor) = true

type DefaultForestContext(viewRegistry:IViewRegistry, securityManager:ISecurityManager) =
    inherit AbstractForestContext(viewRegistry, securityManager)
