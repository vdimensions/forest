namespace Forest.Web.WebSharper

open System
open System.Collections.Concurrent
open Microsoft.AspNetCore.Http
open Axle.Forest
open Axle.Logging
open Axle.Modularity
open Axle.Verification
open Axle.Web.AspNetCore.Session
open Axle.Web.WebSharper
open Forest

type [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] RequiresForestWebSharperAttribute() = 
    inherit RequiresAttribute(typeof<ForestWebSharperModule>)

and [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] UtilizesForestWebSharperAttribute() = 
    inherit UtilizesAttribute(typeof<ForestWebSharperModule>)

and [<Interface;RequiresForestWebSharper>] IWebSharperTemplateConfigurer =
    abstract member Configure: registry : IWebSharperTemplateRegistry -> unit

and [<Sealed;Module;RequiresForest;RequiresWebSharper;RequiresAspNetSession>]
    internal ForestWebSharperModule private (registeredPhysicalViewFacories : ConcurrentDictionary<vname, WebSharperPhysicalView>, forestContext : IForestContext, perSessionForestFacadeProvider : PerSessionWebSharperForestFacade, logger : ILogger) = 
    do Remoting.Init(perSessionForestFacadeProvider)
    public new (forestContext : IForestContext, accessor : IHttpContextAccessor, logger : ILogger) = ForestWebSharperModule(ConcurrentDictionary<_, _>(StringComparer.Ordinal), forestContext, new PerSessionWebSharperForestFacade(accessor), logger)
        
    [<ModuleTerminate>]
    member internal __.Terminated() =
        (perSessionForestFacadeProvider :> IDisposable).Dispose()
        registeredPhysicalViewFacories.Clear()

    [<ModuleDependencyInitialized>]
    member internal this.DependencyInitialized(c : IWebSharperTemplateConfigurer) =
        this |> c.Configure

    interface ISessionEventListener with
        member this.OnSessionStart session =
            let sessionId = session.Id
            perSessionForestFacadeProvider.AddOrReplace(
                sessionId,
                new WebSharperForestFacade(forestContext, WebSharperPhysicalViewRenderer(this)),
                Func<WebSharperForestFacade, WebSharperForestFacade, WebSharperForestFacade>(fun _ _ -> WebSharperForestFacade(forestContext, WebSharperPhysicalViewRenderer(this))))
            logger.Trace("WebSharper forest facade for session {0} created", sessionId);

        member __.OnSessionEnd sessionId =
            match perSessionForestFacadeProvider.TryRemove(sessionId) with
            | (true, _) -> logger.Trace("WebSharper forest facade for session {0} deleted", sessionId)
            | (false, _) -> ignore()

    interface IWebSharperTemplateRegistry with
        member this.Register (NotNullOrEmpty "name" name) (NotNull "factory" factory : WebSharperPhysicalView) =
            ignore <| registeredPhysicalViewFacories.AddOrUpdate(
                name,
                factory,
                Func<string, WebSharperPhysicalView, WebSharperPhysicalView>(fun n _ -> invalidOp(String.Format("A physical view with the provided name '{0}' is already registered", n))))
            upcast this

        member __.Get (NotNull "domNode" name) =
            match registeredPhysicalViewFacories.TryGetValue(name) with
            | (true, value) -> value
            | (false, _) -> invalidOp(String.Format("A physical view with the provided name '{0}' could not be found", name))

    interface IForestFacadeProvider with
        member __.ForestFacade with get() = upcast perSessionForestFacadeProvider
 
    interface IDocumentRenderer with
        member __.Doc() = (perSessionForestFacadeProvider.Current.Renderer :?> IDocumentRenderer).Doc()
