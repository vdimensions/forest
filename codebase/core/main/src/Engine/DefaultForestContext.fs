namespace Forest

open Axle.Verification
open Forest.Reflection
open Forest.Security
open Forest.Templates.Raw


type [<AbstractClass;NoComparison>] AbstractForestContext (viewRegistry : IViewRegistry, securityManager : ISecurityManager, templateProvider : ITemplateProvider) =
    do
        ignore <| ``|NotNull|`` "viewRegistry" viewRegistry
        ignore <| ``|NotNull|`` "securityManager" securityManager
        ignore <| ``|NotNull|`` "templateProvider" templateProvider
    member __.ViewRegistry with get() : IViewRegistry = viewRegistry
    member __.SecurityManager with get() : ISecurityManager = securityManager
    member __.TemplateProvider with get() : ITemplateProvider = templateProvider
    
    interface IForestContext with
        member this.ViewRegistry = this.ViewRegistry
        member this.SecurityManager = this.SecurityManager
        member this.TemplateProvider = this.TemplateProvider

type [<Sealed;NoComparison>] DefaultForestContext(viewFactory : IViewFactory, reflectionProvider : IReflectionProvider, securityManager : ISecurityManager, templateProvider : ITemplateProvider) =
    inherit AbstractForestContext(DefaultViewRegistry(viewFactory, reflectionProvider), securityManager, templateProvider)
