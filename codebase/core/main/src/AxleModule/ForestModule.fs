namespace Forest
open System
open System.Diagnostics
open System.Reflection
open System.Runtime.CompilerServices
open Axle
open Axle.DependencyInjection
open Axle.Logging
open Axle.Modularity
open Forest
open Forest.Reflection
open Forest.Resources
open Forest.Security
open Forest.Templates.Raw
open Forest.UI

type [<Sealed;NoEquality;NoComparison>] private LoggingForestFacade(logger : ILogger, facade : IForestFacade) =
    interface IMessageDispatcher with
        member __.SendMessage message =
            let sw = Stopwatch.StartNew()
            let result = facade.SendMessage message
            sw.Stop()
            logger.Trace("Forest 'SendMessage' operation took {0}ms to complete. ", sw.ElapsedMilliseconds)
            result

    interface ICommandDispatcher with
        member __.ExecuteCommand command target arg =
            let sw = Stopwatch.StartNew()
            let result = facade.ExecuteCommand command target arg
            sw.Stop()
            logger.Trace("Forest 'ExecuteCommand' operation took {0}ms to complete. ", sw.ElapsedMilliseconds)
            result

    interface IForestFacade with
        member __.LoadTree tree =
            let sw = Stopwatch.StartNew()
            let result = facade.LoadTree tree
            sw.Stop()
            logger.Trace("Forest 'LoadTree' operation took {0}ms to complete. ", sw.ElapsedMilliseconds)
            result
type [<Sealed;NoEquality;NoComparison>] private LoggingForestFacadeProvider(logger : ILogger, provider : IForestFacadeProvider) =
    interface IForestFacadeProvider with member __.ForestFacade with get() = upcast LoggingForestFacade(logger, provider.ForestFacade)

[<AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>]
type [<Sealed>] RequiresForestAttribute() = inherit RequiresAttribute(typeof<ForestModule>)

and [<Interface;Module;RequiresForest>] IForestViewProvider =
    abstract member RegisterViews: registry : IViewRegistry -> unit

and [<Sealed;NoEquality;NoComparison;Module;Requires(typeof<ForestResourceModule>)>] 
    internal ForestModule(
        container : IContainer, 
        templateProvider : ITemplateProvider, 
        app : Application, 
        rtp : ResourceTemplateProvider, 
        logger : ILogger) =
    [<DefaultValue>]
    val mutable private _context : IForestContext
    [<DefaultValue>]
    val mutable private _facadeProvider : IForestFacadeProvider 

    [<ModuleInit>]
    member this.Init(e : ModuleExporter) =
        let reflectionProvider =
            match container.TryResolve<IReflectionProvider>() with
            | (true, rp) -> rp
            | (false, _) -> upcast DefaultReflectionProvider()
        let securityManager =
            match container.TryResolve<ISecurityManager>() with
            | (true, sm) -> sm
            | (false, _) -> upcast NoopSecurityManager()
        let viewFactory =
            match null2vopt container.Parent with
            | ValueSome c -> (c, app)
            | ValueNone -> (container, app)
            |> AxleViewFactory
        let context : IForestContext = upcast DefaultForestContext(viewFactory, reflectionProvider, securityManager, templateProvider)
        this._context <- context
        this._facadeProvider <- NoOp.FacadeProvider context

        context |> e.Export<IForestContext> |> ignore
        this |> e.Export<IForestFacade> |> ignore

    [<ModuleDependencyInitialized>]
    member this.DependencyInitialized (viewProvider : IForestViewProvider) =
        (this :> IViewRegistry) |> viewProvider.RegisterViews

    [<ModuleDependencyInitialized>]
    member this.DependencyInitialized (facadeProvider : IForestFacadeProvider) =
        this._facadeProvider <- facadeProvider
        this.MakeDebuggerFacade()

    [<Conditional("DEBUG")>]
    member private this.MakeDebuggerFacade() =
        this._facadeProvider <- (LoggingForestFacadeProvider(logger, this._facadeProvider) :> IForestFacadeProvider)

    interface IForestFacade with
        member this.LoadTree t = 
            let opWatch = Stopwatch.StartNew()
            this._facadeProvider.ForestFacade.LoadTree t
            opWatch.Stop()
            logger.Debug("Forest LoadTemplate operation took {0}ms", opWatch.ElapsedMilliseconds)

    interface IMessageDispatcher with
        member this.SendMessage msg = 
            let opWatch = Stopwatch.StartNew()
            this._facadeProvider.ForestFacade.SendMessage msg
            opWatch.Stop()
            logger.Debug("Forest SendMessage operation took {0}ms", opWatch.ElapsedMilliseconds)

    interface ICommandDispatcher with
        member this.ExecuteCommand cmd target arg = 
            let opWatch = Stopwatch.StartNew()
            this._facadeProvider.ForestFacade.ExecuteCommand cmd target arg
            opWatch.Stop()
            logger.Debug("Forest ExecuteCommand operation took {0}ms", opWatch.ElapsedMilliseconds)

    interface IViewRegistry with
        member this.GetDescriptor(viewType : Type): IViewDescriptor = this._context.ViewRegistry.GetDescriptor viewType
        member this.GetDescriptor(name : vname): IViewDescriptor = this._context.ViewRegistry.GetDescriptor name
        member this.Register<'t when 't:>IView>():IViewRegistry = 
            typeof<'t>.GetTypeInfo().Assembly |> rtp.RegisterAssemblySource 
            this._context.ViewRegistry.Register<'t>()
        member this.Register(t : Type): IViewRegistry = 
            t.GetTypeInfo().Assembly |> rtp.RegisterAssemblySource 
            this._context.ViewRegistry.Register t
        member this.Resolve(descriptor : IViewDescriptor): IView = this._context.ViewRegistry.Resolve descriptor
        member this.Resolve(descriptor : IViewDescriptor, model : obj): IView = this._context.ViewRegistry.Resolve(descriptor, model)

[<Extension>]
type Extensions =
    static member LoadForest(builder : Axle.IApplicationBuilder) =
        builder.Load(typeof<ForestModule>)
