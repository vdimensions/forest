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

type [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] RequiresForestWebSharper() = inherit RequiresAttribute(typeof<ForestWebSharperModule>)

and [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] UtilizesForestWebSharper() = inherit UtilizesAttribute(typeof<ForestWebSharperModule>)
    
and [<Interface;RequiresForestWebSharper>] IWebSharperTemplateConfigurer =
    abstract member Configure: registry : IWebSharperTemplateRegistry -> unit

and [<Sealed>] internal WebSharperForestFacade(forestContext : IForestContext, renderer : WebSharperPhysicalViewRenderer) =
    inherit DefaultForestFacade<WebSharperPhysicalViewWrapper>(forestContext, renderer)
    member __.Renderer with get() = renderer

and [<Sealed>] internal PerSessionWebSharperForestFacade(httpContextAccessor : IHttpContextAccessor) =
    inherit SessionScoped<WebSharperForestFacade>(httpContextAccessor)
    
and Client =
    [<DefaultValue>]
    static val mutable private _facade : PerSessionWebSharperForestFacade voption

    static member internal Init (value : PerSessionWebSharperForestFacade) =
        match Client._facade with
        | ValueNone -> Client._facade <- ValueSome value
        | ValueSome _ -> invalidOp "A forest facade is already initialized"

    static member Facade 
        with get() = 
            match Client._facade with
            | ValueSome f -> f.Current :> IForestFacade
            | ValueNone -> invalidOp "A forest facade has not been initialized yet"

and [<Sealed;Module;RequiresForest;RequiresWebSharper;RequiresAspNetSession>] 
    internal ForestWebSharperModule private (registeredPhysicalViewFacories : ConcurrentDictionary<string, WebSharperPhysicalViewFactory>, forestContext : IForestContext, perSessionForestFacadeProvider : PerSessionWebSharperForestFacade, logger : ILogger) = 
    do Client.Init perSessionForestFacadeProvider
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

    interface IForestFacadeProvider with member __.ForestFacade with get() = upcast perSessionForestFacadeProvider.Current
 
    interface IDocumentRenderer with 
        member __.Doc() = (perSessionForestFacadeProvider.Current.Renderer :> IDocumentRenderer).Doc()
        member __.DocVar() = (perSessionForestFacadeProvider.Current.Renderer :> IDocumentRenderer).DocVar()

    interface IWebSharperTemplateRegistry with
        member this.Register (NotNullOrEmpty "name" name) (NotNull "factory" factory : WebSharperPhysicalViewFactory) =
            ignore <| registeredPhysicalViewFacories.AddOrUpdate(
                name,
                factory,
                Func<string, WebSharperPhysicalViewFactory, WebSharperPhysicalViewFactory>(fun n _ -> invalidOp(String.Format("A physical view with the provided name '{0}' is already registered", n))))
            upcast this

        member __.Get (NotNull "domNode" domNode) =  
            let name = domNode.Name
            match registeredPhysicalViewFacories.TryGetValue(name) with
            | (true, value) -> value(domNode)
            | (false, _) -> invalidOp(String.Format("A physical view with the provided name '{0}' could not be found", name))
