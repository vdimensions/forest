namespace Forest.Web.WebSharper

open System
open Microsoft.AspNetCore.Http
open Axle.Logging
open Axle.Modularity
open Axle.Web.AspNetCore.Session
open Axle.Web.WebSharper
open Forest

type [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] RequiresForestWebSharperAttribute() = 
    inherit RequiresAttribute(typeof<ForestWebSharperModule>)

and [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] UtilizesForestWebSharperAttribute() = 
    inherit UtilizesAttribute(typeof<ForestWebSharperModule>)

and [<Interface;RequiresForestWebSharper>] internal IWebSharperForestFacadeProvider =
    abstract member CreateFacade: IForestContext -> UI.IPhysicalViewRenderer<RemotingPhysicalView> -> WebSharperForestFacadeProxy

and [<Sealed;NoComparison;Module;RequiresForest;RequiresWebSharper;RequiresAspNetSession>]
    internal ForestWebSharperModule private (forestContext : IForestContext, perSessionForestFacadeProvider : PerSessionWebSharperForestFacade, logger : ILogger) = 
    do Remoting.Init(perSessionForestFacadeProvider)
    public new (forestContext : IForestContext, accessor : IHttpContextAccessor, logger : ILogger) = ForestWebSharperModule(forestContext, new PerSessionWebSharperForestFacade(accessor), logger)

    [<DefaultValue>]
    val mutable wsForestFacadeProvider : IWebSharperForestFacadeProvider voption

    [<ModuleTerminate>]
    member internal __.Terminated() =
        (perSessionForestFacadeProvider :> IDisposable).Dispose()

    [<ModuleDependencyInitialized>]
    member this.OnWebSharperForestFacadeProviderInit(wsffp : IWebSharperForestFacadeProvider) =
        match this.wsForestFacadeProvider with
        | ValueNone -> this.wsForestFacadeProvider <- ValueSome wsffp
        | ValueSome _ -> invalidOp "Another IWebSharperForestFacadeProvider has already been set"

    member this.CreateFacade(forestContext, renderer) =
        let wsffp = this.wsForestFacadeProvider |> ValueOption.defaultWith(fun _ -> upcast this)
        wsffp.CreateFacade forestContext renderer
        
    interface ISessionEventListener with
        member this.OnSessionStart session =
            let sessionId = session.Id
            perSessionForestFacadeProvider.AddOrReplace(
                sessionId,
                this.CreateFacade(forestContext, WebSharperPhysicalViewRenderer()),
                Func<WebSharperForestFacadeProxy, WebSharperForestFacadeProxy, WebSharperForestFacadeProxy>(fun _ _ -> this.CreateFacade(forestContext, WebSharperPhysicalViewRenderer())))
            logger.Trace("WebSharper forest facade for session {0} created", sessionId);

        member __.OnSessionEnd sessionId =
            match perSessionForestFacadeProvider.TryRemove(sessionId) with
            | (true, _) -> logger.Trace("WebSharper forest facade for session {0} deleted", sessionId)
            | (false, _) -> ignore()

    interface IForestFacadeProvider with
        member __.ForestFacade 
            with get() = upcast perSessionForestFacadeProvider

    interface IWebSharperForestFacadeProvider with
        member __.CreateFacade forestContext renderer =
            logger.Debug("No WebSharperForestFacadeProvider was registered, using the default one.");
            upcast WebSharperForestFacade(forestContext, renderer) : WebSharperForestFacadeProxy
