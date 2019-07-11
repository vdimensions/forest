namespace Forest.Templates.Raw


type [<Struct;NoComparison>] TemplateDefinition = 
    {
        Name : string; 
        Contents : ViewContents list;
    }

 and [<Struct;NoComparison>] ContentDefinition =
    {
        Placeholder : string;
        Contents : RegionContents list
    }

 and [<Struct;NoComparison>] Template = 
    | Root of TemplateDefinition
    | Mastered of master : string * definition : ContentDefinition list

 and [<Struct;StructuralEquality;NoComparison>] ViewContents =
    | Region of name : string * contents : RegionContents list
    | InlinedTemplate of template : string

 and [<Struct;StructuralEquality;NoComparison>] RegionContents =
    | Placeholder of id : string
    | Template of template : string
    | View of name : string * contents : ViewContents list
    | ClearInstruction