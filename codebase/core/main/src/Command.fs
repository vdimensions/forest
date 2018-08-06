namespace Forest
open System
open System.Reflection

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
        member __.Invoke (arg: obj) (view:IView) : unit =
            mi.Invoke(view, [|arg|]) |> ignore
        interface ICommandDescriptor with
            member __.Name with get() = self.Name
            member __.ArgumentType with get() = self.ArgumentType
            member __.Invoke arg view = self.Invoke arg view