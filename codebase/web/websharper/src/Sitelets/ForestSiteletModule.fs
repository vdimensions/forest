namespace Forest.Web.WebSharper.Sitelets

open System
open Axle.Logging
open Axle.Modularity
open Axle.Verification
open Axle.Web.WebSharper.Sitelets
open Forest
open Forest.Web.WebSharper
open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Server

type [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] RequiresForestSiteletAttribute() = 
    inherit RequiresAttribute(typeof<ForestSiteletModule>)

and [<Sealed;AttributeUsage(AttributeTargets.Class|||AttributeTargets.Interface, Inherited = true, AllowMultiple = false)>] UtilizesForestSiteletAttribute() = 
    inherit UtilizesAttribute(typeof<ForestSiteletModule>)

and [<Interface;RequiresForestSiteletAttribute>] IDocumentOutlineProvider =
    abstract member GetDocumentOutline: header: Doc -> body: Doc -> Doc

and [<Sealed;Module;RequiresForestWebSharper;RequiresWebSharperSitelets>]
    internal ForestSiteletModule (forest : IForestFacade, logger : ILogger) = 
    [<DefaultValue>]
    val mutable private _documentOutlineProvider : IDocumentOutlineProvider voption
    let mutable headerDocs : Doc list = List.empty
    let mutable bodyDocs : Doc list = List.empty
    let mutable afterRenderCallbacks : Doc list = List.empty
    
    [<ModuleDependencyInitialized>]
    member internal this.OnDocumentOutlineProviderInitialized (dop : IDocumentOutlineProvider) =
        this._documentOutlineProvider <-
            match this._documentOutlineProvider with
            | ValueNone -> ValueSome dop
            | ValueSome _ -> invalidOp "A document outline provider has already been assigned"

    interface IDocumentOutlineBuilder with
        member this.AddHeader (NotNull "header" header) = 
            headerDocs <- header::headerDocs
            upcast this
        member this.AddBody (NotNull "body" body) = 
            bodyDocs <- body::bodyDocs
            upcast this
        member this.AddAfterRender (NotNull "callback" callback) = 
            afterRenderCallbacks <- (div [ callback |> on.afterRender ] [])::afterRenderCallbacks
            upcast this

    interface ISiteletProvider with
        member this.RegisterSitelets registry =
            let h = headerDocs |> Seq.rev |> Doc.Concat
            let a = (script [ on.afterRender <@ ClientCode.afterRender @> ] []) :: (afterRenderCallbacks |> List.rev)
            let b = bodyDocs |> List.rev
            let dop a b =
                match this._documentOutlineProvider with
                | ValueSome d -> d.GetDocumentOutline a b
                | ValueNone -> Doc.Empty
            ignore <| registry.RegisterSitelet(ForestSitelet.Run forest dop h (a @ b)) 
      