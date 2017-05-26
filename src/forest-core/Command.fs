namespace Forest
open System
open System.Reflection

[<AttributeUsage(AttributeTargets.Method)>]
[<Sealed>]
type CommandAttribute(name: string) = inherit ForestNodeAttribute(name)

[<Interface>]
type ICommandMetadata = 
    abstract Name: string with get
    abstract ArgumentType: Type with get

[<RequireQualifiedAccessAttribute>]
module Command = 
    [<Sealed>]
    // TODO: argument verfication
    type Metadata(name: string, argType: Type, mi: MethodInfo) as self = 
        member this.Name with get() = name
        member this.ArgumentType with get() = argType
        interface ICommandMetadata with
            member this.Name = self.Name
            member this.ArgumentType = self.ArgumentType

    type Error =
        | NonVoidReturnType of MethodInfo
        | MoreThanOneArgument of MethodInfo
        | MultipleErrors of Error[]