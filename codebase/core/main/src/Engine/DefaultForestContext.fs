namespace Forest

open Axle.Verification
open Forest.ComponentModel
open Forest.Security
open Forest.Templates
open Forest.Engine.Aspects


type [<AbstractClass;NoComparison>] AbstractForestContext (viewFactory : IViewFactory, viewRegistry : IViewRegistry, securityManager : ISecurityManager, templateProvider : ITemplateProvider, aspects: IForestExecutionAspect seq) =
    do
        ignore <| ``|NotNull|`` "viewFactory" viewFactory
        ignore <| ``|NotNull|`` "viewRegistry" viewRegistry
        ignore <| ``|NotNull|`` "securityManager" securityManager
        ignore <| ``|NotNull|`` "templateProvider" templateProvider
    member __.ViewFactory with get() : IViewFactory = viewFactory
    member __.ViewRegistry with get() : IViewRegistry = viewRegistry
    member __.SecurityManager with get() : ISecurityManager = securityManager
    member __.TemplateProvider with get() : ITemplateProvider = templateProvider
    member __.Aspects with get() : IForestExecutionAspect seq = aspects
    
    interface IForestContext with
        member this.ViewFactory = this.ViewFactory
        member this.ViewRegistry = this.ViewRegistry
        member this.SecurityManager = this.SecurityManager
        member this.TemplateProvider = this.TemplateProvider
        member this.Aspects = this.Aspects

type [<Sealed;NoComparison>] DefaultForestContext(viewFactory : IViewFactory, securityManager : ISecurityManager, templateProvider : ITemplateProvider, aspects) =
    inherit AbstractForestContext(viewFactory, Forest.DefaultViewRegistry(viewFactory), securityManager, templateProvider, aspects)
