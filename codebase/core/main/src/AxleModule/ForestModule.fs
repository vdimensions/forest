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

type [<Sealed;NoEquality;NoComparison>] private LoggingForestEngine(logger : ILogger, engine : IForestEngine) =
    inherit ForestEngineDecorator(engine)
    override __.SendMessage engine message =
        let sw = Stopwatch.StartNew()
        let result = engine.SendMessage message
        sw.Stop()
        logger.Trace("Forest 'SendMessage' operation took {0}ms to complete. ", sw.ElapsedMilliseconds)
        result

    override __.ExecuteCommand engine command target arg =
        let sw = Stopwatch.StartNew()
        let result = engine.ExecuteCommand command target arg
        sw.Stop()
        logger.Trace("Forest 'ExecuteCommand' operation took {0}ms to complete. ", sw.ElapsedMilliseconds)
        result

    override __.LoadTree (engine, tree) =
        let sw = Stopwatch.StartNew()
        let result = engine.LoadTree tree
        sw.Stop()
        logger.Trace("Forest 'LoadTree' operation took {0}ms to complete. ", sw.ElapsedMilliseconds)
        result
    override __.LoadTree (engine, tree, msg) =
        let sw = Stopwatch.StartNew()
        let result = engine.LoadTree (tree, msg)
        sw.Stop()
        logger.Trace("Forest 'LoadTree' operation took {0}ms to complete. ", sw.ElapsedMilliseconds)
        result

    override __.RegisterSystemView<'sv when 'sv :> ISystemView> engine =
        let sw = Stopwatch.StartNew()
        let result = engine.RegisterSystemView<'sv>()
        sw.Stop()
        logger.Trace("Forest 'RegisterSystemView' operation took {0}ms to complete. ", sw.ElapsedMilliseconds)
        result

    //override __.Render facade renderer result =
    //    let sw = Stopwatch.StartNew()
    //    let result1 = facade.Render renderer result
    //    sw.Stop()
    //    logger.Trace("Forest 'Render' operation took {0}ms to complete. ", sw.ElapsedMilliseconds)
    //    result1

//type [<Sealed;NoEquality;NoComparison>] private LoggingForestFacadeProvider(logger : ILogger, provider : IForestFacadeProvider) =
//    interface IForestFacadeProvider with member __.ForestFacade with get() = upcast LoggingForestFacade(logger, provider.ForestFacade)

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
    //[<DefaultValue>]
    //val mutable private _facadeProvider : IForestFacadeProvider 
    [<DefaultValue>]
    val mutable private _forestStateManager : ForestStateManager
    [<DefaultValue>]
    val mutable private _forestEngine : IForestEngine
    
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
        this._forestEngine <- ForestExecutionEngine.T() |> ForestEngineStateDecorator.Decorate this._context this._forestStateManager 
        context |> e.Export<IForestContext> |> ignore
        this |> e.Export<IForestEngine> |> ignore

    [<ModuleDependencyInitialized>]
    member this.DependencyInitialized (viewProvider : IForestViewProvider) =
        (this :> IViewRegistry) |> viewProvider.RegisterViews

    [<ModuleDependencyInitialized>]
    member this.DependencyInitialized (stateManager : ForestStateManager) =
        this._forestStateManager <- stateManager
        this._forestEngine <- ForestExecutionEngine.T() |> ForestEngineStateDecorator.Decorate this._context this._forestStateManager
        this.MakeDebuggerFacade()

    [<Conditional("DEBUG")>]
    member private this.MakeDebuggerFacade() =
        this._forestEngine <- LoggingForestEngine(logger, this._forestEngine)

    member private this.ForestEngine with get() = this._forestEngine            

    interface IForestEngine with
        [<Obsolete>]
        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = 
            this.ForestEngine.RegisterSystemView<'sv>()

    interface ITreeNavigator with
        member this.LoadTree t = 
            this.ForestEngine.LoadTree t

        member this.LoadTree (t, m) = 
            this.ForestEngine.LoadTree (t, m)

        //member this.Render renderer result = 
        //    this.ForestEngine.Render renderer result

    interface IMessageDispatcher with
        member this.SendMessage msg = 
            this.ForestEngine.SendMessage msg

    interface ICommandDispatcher with
        member this.ExecuteCommand cmd target arg = 
            this.ForestEngine.ExecuteCommand cmd target arg

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
    [<Extension>]
    static member LoadForest(builder : Axle.IApplicationBuilder) =
        builder.Load(typeof<ForestModule>)
