namespace Forest.Templates.Raw


type [<Struct;NoComparison>] TemplateDefinition = 
    {
        name:string; 
        contents:ViewContents list;
    }
 and [<Struct;NoComparison>] ContentDefinition =
    {
        placeholder:string;
        contents:RegionContents list
    }
 and [<Struct;NoComparison>] Template = 
    | Root of TemplateDefinition
    | Mastered of master:string * definition:ContentDefinition list
 and [<StructuralEquality;NoComparison>] ViewContents =
    | Region of name:string * contents:RegionContents list
    | InlinedTemplate of template:string
 and [<Struct;StructuralEquality;NoComparison>] RegionContents =
    | Placeholder of id:string
    | Template of template:string
    | View of name:string * contents:ViewContents list
    | ClearInstruction