namespace Forest.Resources
open System
open Axle.Modularity
open Axle.Resources
open Axle.Resources.Extraction
open Forest.Resources


[<Module;RequiresResources;Requires(typeof<ForestResourceModule>)>]
type [<Interface>] IForestTemplateMarshallerConfigurer =
    abstract member RegisterMarshallers: registry : IForestTemplateMarshallerRegistry -> unit

 and [<Sealed;NoEquality;NoComparison;Module;RequiresResources>] internal ForestResourceModule =
    val private resourceManager : ResourceManager
    val private templateProvider : ResourceTemplateProvider
    new(rm:ResourceManager) = { resourceManager = rm; templateProvider = ResourceTemplateProvider(rm) }

    [<ModuleInit>]
    member this.Init (e : ModuleExporter) =
        ResourceTemplateProvider.BundleName |> this.templateProvider.AddBundle
        this |> this.ModuleDependencyInitialized
        e.Export(this.templateProvider) |> ignore

    [<ModuleTerminate>]
    member this.Terminate () =
        this |> this.ModuleDependencyTerminated

    [<ModuleDependencyInitialized>]
    member this.ModuleDependencyInitialized (cfg : IForestTemplateMarshallerConfigurer) =
        this |> cfg.RegisterMarshallers

    [<ModuleDependencyTerminated>]
    member __.ModuleDependencyTerminated (_ : IForestTemplateMarshallerConfigurer) =
        ()

    interface IForestTemplateMarshallerConfigurer with
        member __.RegisterMarshallers registry =
            // 
            // Enable support for XML templates out of the box
            //
            XmlTemplateMarshaller() |> registry.Register |> ignore

    interface IForestTemplateMarshallerRegistry with
        member this.Register m =
            let parseUri = (new Axle.Conversion.Parsing.UriParser()).Parse
            let marshallingExtractor = MarshallingTemplateExtractor(m)
            let defaultBundleName = ResourceTemplateProvider.BundleName
            let formatSpecificBundleName = String.Format("{0}/{1}", defaultBundleName, marshallingExtractor.Extension)
            let extractors = marshallingExtractor.ToExtractorList()
            formatSpecificBundleName |> this.templateProvider.AddBundle
            for bundle in [formatSpecificBundleName;defaultBundleName] do
                this.resourceManager.Bundles
                    .Configure(bundle)
                    .Register(String.Format("./{0}", defaultBundleName) |> parseUri)
                    .Register(String.Format("./{0}", formatSpecificBundleName) |> parseUri)
                    .Extractors.Register(extractors)
                    |> ignore
            upcast this:IForestTemplateMarshallerRegistry
