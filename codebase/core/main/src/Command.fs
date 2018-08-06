namespace Forest
open System
open System.Reflection

[<AttributeUsage(AttributeTargets.Method)>]
type [<Sealed>] CommandAttribute(name: string) = inherit ForestNodeAttribute(name)

type [<Interface>] ICommandDescriptor = 
    abstract Name: string with get
    abstract ArgumentType: Type with get

[<RequireQualifiedAccess>]
module Command = 
    type Error =
        | NonVoidReturnType of MethodInfo
        | MoreThanOneArgument of MethodInfo
        | MultipleErrors of Error list

    // TODO: argument verification
    type [<Sealed>] Descriptor(name: string, argType: Type, mi: MethodInfo) as self = 
        member __.Name with get() = name
        member __.ArgumentType with get() = argType
        interface ICommandDescriptor with
            member __.Name with get() = self.Name
            member __.ArgumentType with get() = self.ArgumentType