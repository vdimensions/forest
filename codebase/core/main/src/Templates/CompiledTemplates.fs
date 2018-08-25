namespace Forest.Templates.Compiled

type [<Struct>] Template = {name:string; contents:ViewContents list}
 and [<Struct>] ViewContents =
    | RegionDefinition of name:string * contents:RegionContents list
 and RegionContents =
    | ViewDefinition of name:string * contents:ViewContents list
    | ContentDefinition of placeholder:string

[<RequireQualifiedAccess>]
[<CompiledName("CompiledTemplatesModule")>]
module Compiled =
    let a () = ()