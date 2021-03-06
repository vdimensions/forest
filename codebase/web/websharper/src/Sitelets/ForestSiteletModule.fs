﻿namespace Forest.Web.WebSharper.Sitelets

open System
open System.Collections.Generic
open System.Collections.Concurrent
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Server
open Axle.Logging
open Axle.Modularity
open Axle.Verification
open Axle.Web.WebSharper.Sitelets
open Forest
open Forest.ComponentModel
open Forest.Web.WebSharper
open Forest.Web.WebSharper.UI
open Forest.Engine

type [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] RequiresForestSiteletAttribute() = 
    inherit RequiresAttribute(typeof<ForestSiteletModule>)

and [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] UtilizesForestSiteletAttribute() = 
    inherit UtilizesAttribute(typeof<ForestSiteletModule>)

and [<Interface;RequiresForestSitelet>] IDocumentOutlineProvider =
    abstract member GetDocumentOutline: header: Doc -> body: Doc -> Doc

and [<Interface;RequiresForestSitelet>] IWebSharperTemplateConfigurer =
    abstract member Configure: registry : IWebSharperPhysicalViewRegistry -> unit

and [<Interface;RequiresForestSitelet>] IForestSiteletService =
    abstract member Render : ForestEndPoint<_> -> Async<Content<_>>

and [<Sealed>] internal ForestSiteletService (f : IForestEngine, pvs : IDictionary<string, WebSharperPhysicalView> , dop : (Doc -> Doc -> Doc), h : Doc, b : Doc list) =
    interface IForestSiteletService with 
        member __.Render e = ForestSitelet.Render f (pvs |> Seq.map ``|KeyValue|`` |> Array.ofSeq) dop h b e

and [<Sealed;Module;RequiresForestWebSharper;RequiresWebSharperSitelets;RequiresForest>]
    internal ForestSiteletModule private (registeredPhysicalViewFactories : ConcurrentDictionary<string, WebSharperPhysicalView>, forest : IForestEngine, viewRegistry : IViewRegistry, logger : ILogger) = 
    public new (forest : IForestEngine, viewRegistry : IViewRegistry, logger : ILogger) = ForestSiteletModule(ConcurrentDictionary<_, _>(StringComparer.Ordinal), forest, viewRegistry, logger)
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
            | ValueSome d -> d.GetDocumentOutline a (Doc.Concat(b::afterRenderCallbacks))
            | ValueNone -> Doc.Empty
        ForestSiteletService(forest, registeredPhysicalViewFactories, dop, Doc.Empty, [])
        |> exp.Export
        |> ignore

    [<ModuleTerminate>]
    member internal __.Terminated() =
        registeredPhysicalViewFactories.Clear()

    interface IDocumentOutlineBuilder with
        member this.AddHeader (NotNull "header" header) = 
            headerDocs <- header::headerDocs
            upcast this
        member this.AddAfterRender (NotNull "callback" callback) = 
            afterRenderCallbacks <- (div [ callback |> on.afterRender ] [])::afterRenderCallbacks
            upcast this

    interface IWebSharperPhysicalViewRegistry with
        member this.Register<'PV when 'PV :> WebSharperPhysicalView and 'PV : (new : unit -> 'PV)> () =
            let v = new 'PV()
            let logicalViewType = v.GetLogicalViewType()
            let descriptor = viewRegistry.Register(logicalViewType).GetDescriptor(logicalViewType)
            let n = descriptor.Name
            
            let newExpr = String.Format("new {0}.New()", v.GetClientTypeName())
            afterRenderCallbacks <- (script [ on.afterRender <@ (fun _ -> Client.registerView n (downcast WebSharper.JavaScript.JS.Eval newExpr : WebSharperPhysicalView))@> ] [])::afterRenderCallbacks
            upcast this

    // TODO: implement sitelet-redirect-capable facade and return it here
    //interface IWebSharperForestFacadeProvider with
    //    member this.CreateFacade forestContext renderer =
    //        ?

