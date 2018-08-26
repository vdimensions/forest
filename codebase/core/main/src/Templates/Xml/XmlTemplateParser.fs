namespace Forest.Templates.Xml

open Forest.NullHandling
open Forest.Templates
open Forest.Templates.Raw

open System
open System.Collections.Generic
open System.Xml.Linq


type [<Sealed>] XmlTemplateParser() =
    inherit AbstractTemplateParser()

    let (|TagName|_|) (tag:string) (e:XElement) =
        if StringComparer.Ordinal.Equals(e.Name.LocalName, tag) 
        then Some e
        else None

    member private this.ReadPlaceHolderContents(elements:IEnumerable<XElement>) =
        let mutable result = List.empty
        for e in elements do
            match e with
            | TagName "content" v ->
                let id = v.Attribute("id" |> XName.Get).Value
                let children = v.Elements() |> this.ReadRegionContents
                result <- (this.CreateContentDefinition id children)::result
            | _ -> ignore()
        result |> List.rev

    member private this.ReadViewContents(elements:IEnumerable<XElement>) =
        let mutable result = List.empty
        for e in elements do
            match e with
            | TagName "region" v ->
                let name = v.Attribute("name" |> XName.Get).Value
                let children = v.Elements() |> this.ReadRegionContents
                result <- (this.CreateRegion name children)::result
            | TagName "inline" v ->
                let name = v.Attribute("template" |> XName.Get).Value
                result <- (this.CreateInlinedTemplate name)::result
            | _ -> ignore()
        result |> List.rev
    member private this.ReadRegionContents(elements:IEnumerable<XElement>) =
        let mutable result = List.empty
        for e in elements do
            match e with
            | TagName "clear" _ ->
                result <- ClearInstruction::result
            | TagName "view" v ->
                let name = v.Attribute("name" |> XName.Get).Value
                let contents = v.Elements() |> this.ReadViewContents
                result <- (this.CreateView name contents)::result
            | TagName "placeholder" v ->
                let id = v.Attribute("id" |> XName.Get).Value
                result <- (this.CreatePlaceholder id)::result
            | TagName "template" v ->
                let id = v.Attribute("name" |> XName.Get).Value
                result <- (this.CreateTemplate id)::result
            | _ -> ignore()
        result |> List.rev

    override this.Parse name stream =
        let template = XDocument.Load(stream, LoadOptions.None).Root
        let master = template.Attribute("master" |> XName.Get)
        match null2vopt master with
        | ValueSome master -> Mastered(master.Value, this.ReadPlaceHolderContents(template.Elements()))
        | ValueNone -> this.ReadViewContents(template.Elements()) |> this.CreateTemplateDefinition name |> Root


