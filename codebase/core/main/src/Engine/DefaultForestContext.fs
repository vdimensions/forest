namespace Forest

open Forest.Reflection
open Forest.Security
open Forest.Templates.Raw


type [<AbstractClass>] AbstractForestContext (viewRegistry:IViewRegistry, securityManager:ISecurityManager, templateProvider:ITemplateProvider) =
    member __.ViewRegistry with get() : IViewRegistry = viewRegistry
    member __.SecurityManager with get() : ISecurityManager = securityManager
    member __.TemplateProvider with get() : ITemplateProvider = templateProvider
    interface IForestContext with
        member this.ViewRegistry = this.ViewRegistry
        member this.SecurityManager = this.SecurityManager
        member this.TemplateProvider = this.TemplateProvider

type NoopSecurityManager() =
    interface ISecurityManager with
        member __.HasAccess(_:ICommandDescriptor) = true
        member __.HasAccess(_:IViewDescriptor) = true

type DefaultForestContext(viewFactory:IViewFactory, reflectionProvider:IReflectionProvider, securityManager:ISecurityManager, templateProvider:ITemplateProvider) =
    inherit AbstractForestContext(DefaultViewRegistry(viewFactory, reflectionProvider), securityManager, templateProvider)
