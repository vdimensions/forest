namespace Forest.Web.WebSharper

open System
open Microsoft.AspNetCore.Http
open Axle.Logging
open Axle.Modularity
open Axle.Web.AspNetCore.Session
open Axle.Web.WebSharper
open Forest
open Forest.Engine

type [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] RequiresForestWebSharperAttribute() = 
    inherit RequiresAttribute(typeof<ForestWebSharperModule>)

and [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] UtilizesForestWebSharperAttribute() = 
    inherit UtilizesAttribute(typeof<ForestWebSharperModule>)

and [<Sealed;NoComparison;Module;RequiresForest;RequiresWebSharper;RequiresAspNetSession>]
    internal ForestWebSharperModule private (forest : IForestEngine, wsssp : WebSharperSessionStateProvider, logger : ILogger) = 
    do Remoting.Init(forest, wsssp)
    public new (forest : IForestEngine, accessor : IHttpContextAccessor, logger : ILogger) = ForestWebSharperModule(forest, new WebSharperSessionStateProvider(accessor), logger)

    [<ModuleTerminate>]
    member internal __.Terminated() =
        (wsssp :> IDisposable).Dispose()

    interface ISessionEventListener with
        member __.OnSessionStart session =
            let sessionId = session.Id
            wsssp.AddOrReplace(
                sessionId,
                WebSharperForestState(),
                Func<WebSharperForestState, WebSharperForestState, WebSharperForestState>(fun _ newState -> newState))
            logger.Trace("WebSharper forest facade for session {0} created", sessionId);

        member __.OnSessionEnd sessionId =
            match wsssp.TryRemove(sessionId) with
            | (true, _) -> logger.Trace("WebSharper forest facade for session {0} deleted", sessionId)
            | (false, _) -> ignore()

    interface IForestIntegrationProvider with
        member __.Renderer with get() = upcast wsssp
        member __.StateProvider with get() = upcast wsssp
