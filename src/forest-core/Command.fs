namespace Forest
open System
open System.Reflection

[<AttributeUsage(AttributeTargets.Method)>]
[<Sealed>]
type CommandAttribute(name: string) = inherit ForestNodeAttribute(name)

[<RequireQualifiedAccessAttribute>]
module Command = 
    [<Sealed>]
    type Metadata(name: string, argType: Type, mi: MethodInfo) = 
        member this.Name with get() = name
        member this.ArgumentType with get() = argType

    type Error =
    | NonVoidReturnType of MethodInfo
    | MoreThanOneArgument of MethodInfo
    | MultipleErrors of Error[]