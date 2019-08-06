namespace Forest.Templates.Raw


[<RequireQualifiedAccess>]
module Raw =
    /// <summary>
    /// Expands a given <c>template</c>'s hierarchy to a <see cref="TemplateDefinition">template definition</see> 
    /// list with the top-hierarchy root the last
    /// </summary>
    let rec private expand (provider : ITemplateProvider) (template : Template) : Template list =
        match template with
        | Root _ -> [template]
        | Mastered (master, _) -> template::(master |> provider.Load |> expand provider)

    let rec loadTemplate (provider : ITemplateProvider) (name : string) =
        let hierarchy =
            name 
            |> provider.Load 
            |> expand provider 
            |> List.rev 
        { Name = name; Contents = List.empty } |> flattenTemplate provider hierarchy

    and private flattenTemplate (provider : ITemplateProvider) (templates : Template list) (result : TemplateDefinition) : TemplateDefinition =
        match templates with
        | [] -> result
        | head::tail -> 
            let (placeholderMap, res) = 
                match head with
                | Root d -> (Map.empty, { result with Contents = d.Contents; Name = d.Name })
                | Mastered (_, contentDefinitions) ->
                    let placeholderMap = 
                        contentDefinitions 
                        |> Seq.map (fun a -> (a.Placeholder, a.Contents)) 
                        |> Map.ofSeq
                    (placeholderMap, result)
            let newContents = 
                res.Contents 
                |> inlineTemplates provider
                |> processPlaceholders placeholderMap
                |> expandTemplates provider
            { res with Contents = newContents } |> flattenTemplate provider tail
    /// Locates and expands any `InlinedTemplate` item
    and private inlineTemplates (provider : ITemplateProvider) (current : ViewContents list) : ViewContents list =
        [
            for vc in current do
                match vc with 
                | InlinedTemplate t -> 
                    let inlinedTemplate = t |> loadTemplate provider
                    for c in (inlineTemplates provider inlinedTemplate.Contents) do 
                        yield c
                | Region (name, regionContents) ->
                    let mutable newRegionContents = List.empty
                    for rc in regionContents do
                        match rc with
                        | ClearInstruction -> newRegionContents <- List.empty
                        | Placeholder _ -> newRegionContents <- rc::newRegionContents
                        | View (name, nestedViewContents) ->
                            let newNestedViewContents = nestedViewContents |> inlineTemplates provider 
                            newRegionContents <- View (name, newNestedViewContents)::newRegionContents
                        | any -> newRegionContents <- any::newRegionContents
                    yield Region (name, List.rev newRegionContents)
        ]

    /// Inlines the contents of any accumulated `Placeholders` into the respective `Content` items
    and private processPlaceholders (placeholderData : Map<string, RegionContents list>) (current : ViewContents list) : ViewContents list =
        if placeholderData |> Map.isEmpty |> not then
            let mutable result = List.empty
            let mutable pd = placeholderData
            for vc in current do
                match vc with 
                | InlinedTemplate _ -> result <- vc::result
                | Region (name, regionContents) ->
                    let mutable newRegionContents = List.empty
                    for rc in regionContents do
                        match rc with
                        | ClearInstruction -> newRegionContents <- List.empty
                        | Placeholder placeholder ->
                            match pd.TryFind placeholder with
                            | None -> newRegionContents <- rc::newRegionContents
                            | Some placeholderContents -> 
                                for placeholdedContent in placeholderContents do 
                                    newRegionContents <- placeholdedContent::newRegionContents
                                pd <- pd.Remove placeholder
                        | View (name, nestedViewContents) ->
                            let newNestedViewContents = nestedViewContents |> processPlaceholders pd 
                            newRegionContents <- View(name, newNestedViewContents)::newRegionContents
                        | Template _ -> newRegionContents <- rc::newRegionContents
                    result <- Region (name, List.rev newRegionContents)::result
            result
        else current

    /// expands any `Template` items to a fully flattened template 
    and private expandTemplates (provider : ITemplateProvider) (current : ViewContents list) : ViewContents list =
        let mutable result = List.empty
        for vc in current do
            match vc with 
            | Region (name, regionContents) ->
                let mutable newRegionContents = List.empty
                for rc in regionContents do
                    match rc with
                    | ClearInstruction -> newRegionContents <- List.empty
                    | Placeholder _ -> newRegionContents <- rc::newRegionContents
                    | View (name, nestedViewContents) ->
                        let newNestedViewContents = nestedViewContents |> expandTemplates provider 
                        newRegionContents <- View (name, newNestedViewContents)::newRegionContents
                    | Template t -> newRegionContents <- View(t, (loadTemplate provider t).Contents)::newRegionContents
                result <- Region (name, List.rev newRegionContents)::result
            | any -> result <- any::result
        result
