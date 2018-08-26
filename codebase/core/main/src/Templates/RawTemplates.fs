namespace Forest.Templates.Raw


type [<Struct>] TemplateDefinition = 
    {
        [<CompiledName("Name")>] 
        name:string; 
        [<CompiledName("Contents")>] 
        contents:ViewContents list;
    }
 and [<Struct>] ContentDefinition =
    {
        [<CompiledName("Placeholder")>] 
        placeholder:string;
        [<CompiledName("Contents")>] 
        contents:RegionContents list
    }
 and [<Struct>] Template = 
    | Root of TemplateDefinition
    | Mastered of master:string * definition:ContentDefinition list
 and ViewContents =
    | Region of name:string * contents:RegionContents list
    | InlinedTemplate of template:string
    //| Content of definition:ContentDefinition
 and [<Struct>] RegionContents =
    | Placeholder of id:string
    | Template of template:string
    | View of name:string * contents:ViewContents list
    | ClearInstruction

type [<Interface>] ITemplateProvider =
    abstract member Load: name:string -> Template

[<RequireQualifiedAccess>]
[<CompiledName("RawTemplatesModule")>]
module Raw =
    /// <summary>
    /// Expands a given <c>template</c>'s hierarchy to a <see cref="TemplateDefinition">template definition</see> 
    /// list with the top-hierarchy root the last
    /// </summary>
    let rec private expand (provider:ITemplateProvider) (template:Template) : Template list =
        match template with
        | Root _ -> [template]
        | Mastered (master, _) -> template::(master |> provider.Load |> expand provider)

    let rec loadTemplate (provider:ITemplateProvider) (name:string) =
        let hierarchy =
            name 
            |> provider.Load 
            |> expand provider 
            |> List.rev 
        { name = name; contents = List.empty } |> flattenTemplate provider hierarchy

    and private flattenTemplate (provider:ITemplateProvider) (templates:Template list) (result:TemplateDefinition) : TemplateDefinition =
        match templates with
        | [] -> result
        | head::tail -> 
            match head with
            | Root d -> { result with contents = d.contents }
            | Mastered (_, d) ->
                let placeholderMap = d |> Seq.map (fun a -> (a.placeholder, a.contents)) |> Map.ofSeq
                let newContents = 
                     result.contents 
                     |> inlineTemplates provider
                     |> processPlaceholders placeholderMap
                     |> expandTemplates provider
                { result with contents = newContents }
            |> flattenTemplate provider tail
    /// Locates and expands any `InlinedTemplate` item
    and private inlineTemplates (provider:ITemplateProvider) (current:ViewContents list):ViewContents list =
        let mutable result = List.empty
        for vc in current do
            match vc with 
            | InlinedTemplate t -> 
                let inlinedTemplate = t |> loadTemplate provider
                for c in (inlineTemplates provider inlinedTemplate.contents) do 
                    result <- c::result
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
                result <- Region (name, newRegionContents |> List.rev)::result
        result |> List.rev
    /// Inlines the contents of any accumulated `Placeholders` into the respective `Content` items
    and private processPlaceholders (placeholderData:Map<string, RegionContents list>) (current:ViewContents list):ViewContents list =
        if placeholderData |> Map.isEmpty |> not 
        then
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
                            newRegionContents <- View (name, newNestedViewContents)::newRegionContents
                        | Template _ -> newRegionContents <- rc::newRegionContents
                    result <- Region (name, newRegionContents |> List.rev)::result
            result |> List.rev
        else current
    /// expands any `Template` items to a fully flattened template 
    and private expandTemplates (provider:ITemplateProvider) (current:ViewContents list):ViewContents list =
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
                    | Template t -> newRegionContents <- View(t, (loadTemplate provider t).contents)::newRegionContents
                result <- Region (name, newRegionContents |> List.rev)::result
            | any -> result <- any::result
        result |> List.rev
