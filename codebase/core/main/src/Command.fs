namespace Forest
open System
open System.Reflection

[<AttributeUsage(AttributeTargets.Method)>]
type [<Sealed>] CommandAttribute(name: string) = inherit ForestNodeAttribute(name)

type [<Interface>] ICommandDescriptor = 
    abstract Name: string with get
    abstract ArgumentType: Type with get

[<RequireQualifiedAccessAttribute>]
module Command = 
    // TODO: argument verification
    type [<Sealed>] Descriptor(name: string, argType: Type, mi: MethodInfo) as self = 
        member this.Name with get() = name
        member this.ArgumentType with get() = argType
        interface ICommandDescriptor with
            member this.Name = self.Name
            member this.ArgumentType = self.ArgumentType

    type Error =
        | NonVoidReturnType of MethodInfo
        | MoreThanOneArgument of MethodInfo
        | MultipleErrors of Error[]