namespace Forest.Templates

open Forest.Templates.Raw

type [<Interface>] ITemplateParser =
    abstract member Parse: name : string -> stream : System.IO.Stream -> Template

type [<AbstractClass>] AbstractTemplateParser() =
    abstract member Parse: name : string -> stream : System.IO.Stream -> Template
    member __.CreateTemplateDefinition (name : string) (contents : ViewContents list) : TemplateDefinition =
        { Name = name; Contents = contents }
    member __.CreateRoot (definition : TemplateDefinition) : Template =
        Template.Root definition
    member __.CreateMastered (master : string) (definition : ContentDefinition list) : Template =
        Template.Mastered(master, definition)
    member __.CreateContentDefinition (placeholder : string) (contents : RegionContents list) : ContentDefinition =
        { Placeholder = placeholder; Contents = contents }
    member __.CreatePlaceholder (placeholder : string) : RegionContents =
        RegionContents.Placeholder placeholder
    member __.CreateTemplate (name : string) : RegionContents =
        RegionContents.Template name
    member __.CreateView (name : string) (contents : ViewContents list) : RegionContents =
        RegionContents.View(name, contents)
    member __.CreateRegion (name : string) (contents : RegionContents list) : ViewContents =
        ViewContents.Region(name, contents)
    member __.CreateInlinedTemplate (name : string) : ViewContents =
        ViewContents.InlinedTemplate(name)
    interface ITemplateParser with member this.Parse name stream = this.Parse name stream