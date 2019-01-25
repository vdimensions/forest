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

and [<Sealed;NoComparison;Module;RequiresForest;RequiresWebSharper;RequiresAspNetSession>]
    internal ForestWebSharperModule private (forestContext : IForestContext, perSessionForestFacadeProvider : PerSessionWebSharperForestFacade, logger : ILogger) = 
    do Remoting.Init(perSessionForestFacadeProvider)
    public new (forestContext : IForestContext, accessor : IHttpContextAccessor, logger : ILogger) = ForestWebSharperModule(forestContext, new PerSessionWebSharperForestFacade(accessor), logger)

    [<ModuleTerminate>]
    member internal __.Terminated() =
        (perSessionForestFacadeProvider :> IDisposable).Dispose()
        
    interface ISessionEventListener with
        member __.OnSessionStart session =
            let sessionId = session.Id
            perSessionForestFacadeProvider.AddOrReplace(
                sessionId,
                new WebSharperForestFacade(forestContext, WebSharperPhysicalViewRenderer()),
                Func<WebSharperForestFacade, WebSharperForestFacade, WebSharperForestFacade>(fun _ _ -> WebSharperForestFacade(forestContext, WebSharperPhysicalViewRenderer())))
            logger.Trace("WebSharper forest facade for session {0} created", sessionId);

        member __.OnSessionEnd sessionId =
            match perSessionForestFacadeProvider.TryRemove(sessionId) with
            | (true, _) -> logger.Trace("WebSharper forest facade for session {0} deleted", sessionId)
            | (false, _) -> ignore()

    interface IForestFacadeProvider with
        member __.ForestFacade 
            with get() = upcast perSessionForestFacadeProvider
