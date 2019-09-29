namespace Forest
open System
open System.Reflection
open System.Runtime.CompilerServices
open Axle
open Axle.DependencyInjection
open Axle.Logging
open Axle.Modularity
open Axle.Verification
open Forest
open Forest.ComponentModel
open Forest.Engine
open Forest.Engine.Aspects
open Forest.Security
open Forest.StateManagement
open Forest.Templates
open Forest.UI
open System.Diagnostics

type [<Interface>] IForestIntegrationProvider =
    abstract member Renderer : IPhysicalViewRenderer with get
    abstract member StateProvider : IForestStateProvider with get

[<AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>]
type [<Sealed>] RequiresForestAttribute() = inherit RequiresAttribute(typeof<ForestModule>)

and [<Interface;Module;RequiresForest>] IForestViewProvider =
    abstract member RegisterViews: registry : IViewRegistry -> unit

and [<Sealed;NoEquality;NoComparison;Module;Requires(typeof<ForestTemplatesModule>)>] 
    internal ForestModule(
        container : IContainer, 
        templateProvider : ITemplateProvider, 
        app : Application, 
        rtp : ResourceTemplateProvider, 
        logger : ILogger) =
    [<DefaultValue>]
    val mutable private _context : IForestContext
    [<DefaultValue>]
    val mutable private _integrationProvider : IForestIntegrationProvider voption
    //[<DefaultValue>]
    //val mutable private _forestEngine : IForestEngine

    //member this.InitForest() =
        //let pvr, sp =
        //    match this._integrationProvider with
        //    | ValueSome ip ->
        //        ip.Renderer, ip.StateProvider
        //    | ValueNone ->
        //        (NoOp.PhysicalViewRenderer() :> IPhysicalViewRenderer, DefaultForestStateProvider() :> IForestStateProvider)
        //this._forestEngine <- ForestEngine.Create this._context sp pvr logger
    
    [<ModuleInit>]
    member this.Init(e : ModuleExporter) =
        let securityManager =
            match container.TryResolve<ISecurityManager>() with
            | (true, sm) -> sm
            | (false, _) -> upcast NoopSecurityManager()
        let viewFactory =
            match null2vopt container.Parent with
            | ValueSome c -> (c, app)
            | ValueNone -> (container, app)
            |> ContainerViewFactory
        let context : IForestContext = upcast DefaultForestContext(viewFactory, securityManager, templateProvider, seq { yield this :> IForestExecutionAspect })
        this._context <- context
        //this.InitForest()
        context |> e.Export<IForestContext> |> ignore
        this |> e.Export<IForestEngine> |> ignore


    [<ModuleDependencyInitialized>]
    member this.DependencyInitialized (viewProvider : IForestViewProvider) =
        (this :> IViewRegistry) |> viewProvider.RegisterViews

    [<ModuleDependencyInitialized>]
    member this.DependencyInitialized (integration : IForestIntegrationProvider) =
        match this._integrationProvider with
        | ValueNone ->
            this._integrationProvider <- ValueSome integration
        | ValueSome _ ->
            invalidOp "Forest integration is already configured"
        //this.InitForest()

    //member private this.ForestEngine with get() = this._forestEngine   
    
    member private this.CreateEngine() =
        let pvr, sp =
            match this._integrationProvider with
            | ValueSome ip ->
                ip.Renderer, ip.StateProvider
            | ValueNone ->
                (NoOp.PhysicalViewRenderer() :> IPhysicalViewRenderer, DefaultForestStateProvider() :> IForestStateProvider)
        upcast new ForestEngine(this._context, sp, pvr) : IForestEngine

    interface IForestEngine with
        [<Obsolete>]
        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = 
            //this.ForestEngine.RegisterSystemView<'sv>()
            this.CreateEngine().RegisterSystemView<'sv>()

    interface ITreeNavigator with
        member this.Navigate t = 
            //this.ForestEngine.Navigate t
            this.CreateEngine().Navigate t

        member this.Navigate (t, m) = 
            //this.ForestEngine.Navigate (t, m)
            this.CreateEngine().Navigate (t, m)

    interface IMessageDispatcher with
        member this.SendMessage msg = 
            //this.ForestEngine.SendMessage msg
            this.CreateEngine().SendMessage (msg)

    interface ICommandDispatcher with
        member this.ExecuteCommand (cmd, target, arg) = 
            //this.ForestEngine.ExecuteCommand (cmd, target, arg)
            this.CreateEngine().ExecuteCommand (cmd, target, arg)

    interface IViewRegistry with
        member this.GetDescriptor(viewType : Type) : IViewDescriptor = this._context.ViewRegistry.GetDescriptor viewType
        member this.GetDescriptor(name : vname) : IViewDescriptor = this._context.ViewRegistry.GetDescriptor name
        member this.Register<'t when 't:>IView>() :IViewRegistry = 
            typeof<'t>.GetTypeInfo().Assembly |> rtp.RegisterAssemblySource 
            this._context.ViewRegistry.Register<'t>()
        member this.Register(t : Type) : IViewRegistry = 
            t.GetTypeInfo().Assembly |> rtp.RegisterAssemblySource 
            this._context.ViewRegistry.Register t
    interface IViewFactory with
        member this.Resolve(descriptor : IViewDescriptor): IView = this._context.ViewFactory.Resolve descriptor
        member this.Resolve(descriptor : IViewDescriptor, model : obj): IView = this._context.ViewFactory.Resolve(descriptor, model)

    interface IForestExecutionAspect with
        member __.Navigate(cutPoint: IForestExecutionCutPoint) =
            let sw = Stopwatch.StartNew()
            cutPoint.Proceed()
            sw.Stop()
            logger.Trace("Forest Navigate operation took {0}ms", sw.ElapsedMilliseconds)
        member __.ExecuteCommand(cutPoint: IForestExecutionCutPoint) =
            let sw = Stopwatch.StartNew()
            cutPoint.Proceed()
            sw.Stop()
            logger.Trace("Forest ExecuteCommand operation took {0}ms", sw.ElapsedMilliseconds)
        member __.SendMessage(cutPoint: IForestExecutionCutPoint) =
            let sw = Stopwatch.StartNew()
            cutPoint.Proceed()
            sw.Stop()
            logger.Trace("Forest SendMessage operation took {0}ms", sw.ElapsedMilliseconds)

[<Extension>]
type Extensions =
    [<Extension>]
    static member UseForest(NotNull "builder" builder : Axle.IApplicationBuilder) =
        builder.ConfigureModules(fun m -> m.Load(typeof<ForestModule>) |> ignore)
