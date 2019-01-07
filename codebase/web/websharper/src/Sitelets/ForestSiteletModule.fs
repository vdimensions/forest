namespace Forest.Web.WebSharper.Sitelets

open System
open Axle.Logging
open Axle.Modularity
open Axle.Web.WebSharper
open Forest
open Forest.Web.WebSharper

type [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] RequiresForestSiteletAttribute() = 
    inherit RequiresAttribute(typeof<ForestSiteletModule>)

and [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] UtilizesForestSiteletAttribute() = 
    inherit UtilizesAttribute(typeof<ForestSiteletModule>)

and [<Interface;RequiresForestSiteletAttribute>] IDocumentOutlineProvider =
    interface end
    // TODO header doc
    // TODO body doc
    // TODO after-render hook

and [<Sealed;Module;RequiresForestWebSharper;RequiresWebSharperSitelets>]
    internal ForestSiteletModule (forest : IForestFacade, logger : ILogger) = 
    [<DefaultValue>]
    val mutable _documentOutlineProvider : IDocumentOutlineProvider voption
    
    [<ModuleDependencyInitialized>]
    member internal this.DocumentOutlineProviderInitialized (dop : IDocumentOutlineProvider) =
        this._documentOutlineProvider <-
            match this._documentOutlineProvider with
            | ValueNone -> ValueSome dop
            | ValueSome _ -> invalidOp "A document outline provider has already been assigned"
    //public new (forestContext : IForestContext, accessor : IHttpContextAccessor, logger : ILogger) = ForestSiteletModule(ConcurrentDictionary<_, _>(StringComparer.Ordinal), forestContext, new PerSessionWebSharperForestFacade(accessor), logger)
      