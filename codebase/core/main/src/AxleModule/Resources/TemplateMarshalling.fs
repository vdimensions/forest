namespace Forest.Resources
open System
open Axle
open Axle.References
open Axle.Resources
open Axle.Resources.Extraction
open Forest.Templates.Raw


type [<Interface>] IForestTemplateMarshaller =
    abstract member Unmarshal: name : string -> ResourceInfo -> Nullable<Template>
    abstract ChainedExtractors : IResourceExtractor seq with get
    abstract Extension : string with get

type [<Interface>] IForestTemplateMarshallerRegistry =
    abstract member Register: marshaller : IForestTemplateMarshaller -> IForestTemplateMarshallerRegistry

type [<Sealed;NoComparison>] private MarshallingTemplateExtractor (marshaller : IForestTemplateMarshaller) =
    inherit AbstractResourceExtractor()
    override this.DoExtract (ctx : ResourceContext, name : string) =
        let baseResource = String.Format("{0}.{1}", name, this.Extension) |> ctx.ExtractionChain.Extract
        match baseResource |> ns2vopt with
        | ValueSome baseRes ->
            (baseRes 
            |> marshaller.Unmarshal name 
            |> Nullable.map (fun template -> Nullsafe.Some(upcast TemplateResourceInfo(name, ctx.Culture, template, baseRes):ResourceInfo))
            ).GetValueOrDefault()
        | ValueNone -> Nullsafe.None
    member this.ToExtractorList() = 
        (upcast this:IResourceExtractor)::List.ofSeq(marshaller.ChainedExtractors)
    member __.Extension 
        with get() = marshaller.Extension.TrimStart('.')

