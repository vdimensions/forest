namespace Forest.Web.WebSharper.Sitelets

open System
open System.Collections.Generic
open System.Collections.Concurrent
open Axle.Logging
open Axle.Modularity
open Axle.Verification
open Axle.Web.WebSharper.Sitelets
open Forest
open Forest.Web.WebSharper
open Forest.Web.WebSharper.UI
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Server

type [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] RequiresForestSiteletAttribute() = 
    inherit RequiresAttribute(typeof<ForestSiteletModule>)

and [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] UtilizesForestSiteletAttribute() = 
    inherit UtilizesAttribute(typeof<ForestSiteletModule>)

and [<Interface;RequiresForestSitelet>] IDocumentOutlineProvider =
    abstract member GetDocumentOutline: header: Doc -> body: Doc -> Doc

and [<Interface;RequiresForestSitelet>] IWebSharperTemplateConfigurer =
    abstract member Configure: registry : IWebSharperTemplateRegistry -> unit

and [<Interface;RequiresForestSitelet>] IForestSiteletService =
    abstract member Render : ForestEndPoint<_> -> Async<Content<_>>

and [<Sealed>] internal ForestSiteletService (f : IForestFacade, pvs : IDictionary<vname, WebSharperPhysicalView> , dop : (Doc -> Doc -> Doc), h : Doc, b : Doc list) =
    interface IForestSiteletService with 
        member __.Render e = ForestSitelet.Render f (pvs |> Seq.map ``|KeyValue|`` |> Array.ofSeq) dop h b e

and [<Sealed;Module;RequiresForestWebSharper;RequiresWebSharperSitelets>]
    internal ForestSiteletModule private (registeredPhysicalViewFacories : ConcurrentDictionary<vname, WebSharperPhysicalView>, forest : IForestFacade, logger : ILogger) = 
    public new (forest : IForestFacade, logger : ILogger) = ForestSiteletModule(ConcurrentDictionary<_, _>(StringComparer.Ordinal), forest, logger)
    [<DefaultValue>]
    val mutable private _documentOutlineProvider : IDocumentOutlineProvider voption
    let mutable headerDocs : Doc list = List.empty
    let mutable afterRenderCallbacks : Doc list = List.empty
    
    [<ModuleDependencyInitialized>]
    member internal this.OnDocumentOutlineProviderInitialized (dop : IDocumentOutlineProvider) =
        this._documentOutlineProvider <-
            match this._documentOutlineProvider with
            | ValueNone -> ValueSome dop
            | ValueSome _ -> invalidOp "A document outline provider has already been assigned"
    
    [<ModuleDependencyInitialized>]
    member internal this.DependencyInitialized(c : IWebSharperTemplateConfigurer) =
        this |> c.Configure

    [<ModuleInit>]
    member internal this.Init(exp : ModuleExporter) =
        let dop a b =
            match this._documentOutlineProvider with
            | ValueSome d -> d.GetDocumentOutline a b
            | ValueNone -> Doc.Empty
        ForestSiteletService(forest, registeredPhysicalViewFacories, dop, Doc.Empty, [])
        |> exp.Export
        |> ignore
    [<ModuleTerminate>]
    member internal __.Terminated() =
        registeredPhysicalViewFacories.Clear()

    interface IDocumentOutlineBuilder with
        member this.AddHeader (NotNull "header" header) = 
            headerDocs <- header::headerDocs
            upcast this
        member this.AddAfterRender (NotNull "callback" callback) = 
            afterRenderCallbacks <- (div [ callback |> on.afterRender ] [])::afterRenderCallbacks
            upcast this

    interface IWebSharperTemplateRegistry with
        member this.Register (NotNullOrEmpty "name" name, NotNull "physicalView" physicalView : WebSharperPhysicalView) =
            ignore <| registeredPhysicalViewFacories.AddOrUpdate(
                name,
                physicalView,
                Func<string, WebSharperPhysicalView, WebSharperPhysicalView>(fun n _ -> invalidOp(String.Format("A physical view with the provided name '{0}' is already registered", n))))
            //ClientCode.registerView name physicalViewExpr.m
            upcast this
        member this.Register<'PV when 'PV :> WebSharperPhysicalView and 'PV : (new : unit -> 'PV)> (NotNullOrEmpty "name" name) =
            let n = name
            ignore <| registeredPhysicalViewFacories.AddOrUpdate(
                n,
                new 'PV(),
                Func<string, WebSharperPhysicalView, WebSharperPhysicalView>(fun n _ -> invalidOp(String.Format("A physical view with the provided name '{0}' is already registered", n))))
            afterRenderCallbacks <- (script [ on.afterRender <@ (fun _ -> Client.registerView n (WebSharper.JavaScript.JS.New<'PV>()))@> ] [])::afterRenderCallbacks
            upcast this
        //member this.Register (NotNullOrEmpty "name" name, NotNull "physicalViewExpr" physicalViewExpr : Quotations.Expr<#WebSharperPhysicalView>) =
        //    let n = name
        //    
        //    afterRenderCallbacks <- (script [ on.afterRender <@ (fun _ -> Client.registerView n <@ physicalViewExpr @>)@> ] [])::afterRenderCallbacks
        //    upcast this
