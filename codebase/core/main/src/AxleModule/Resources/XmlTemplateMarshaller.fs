namespace Forest.Resources
open Axle.Resources
open Axle.Resources.Xml
open Axle.Resources.Xml.Extraction
open Forest.Templates.Xml


type [<Sealed;NoComparison>] XmlTemplateMarshaller() =
    interface IForestTemplateMarshaller with
        member __.Extension with get() = "xml"
        member __.Unmarshal name resource =
            match resource with
            | :? XDocumentResourceInfo as res -> 
                res.Value 
                |> XmlTemplateParser().ParseXml name 
                |> System.Nullable
            | :? BinaryResourceInfo as res -> 
                use stream = res.Open()
                stream 
                |> XmlTemplateParser().Parse name
                |> System.Nullable
            | _ -> System.Nullable<_>()
        member __.ChainedExtractors with get() = seq { yield upcast XDocumentExtractor() }