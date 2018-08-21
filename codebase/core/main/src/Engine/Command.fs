namespace Forest
open System
open System.Reflection

[<RequireQualifiedAccess>]
module Command = 
    type [<Struct>] Error =
        | NonVoidReturnType of methodWithReturnValue: MethodInfo
        | MoreThanOneArgument of multiArgumentMethod: MethodInfo
        | MultipleErrors of errors: Error list

    // TODO: argument verification
    type [<Sealed>] internal Descriptor(name: string, argType: Type, mi: MethodInfo) as self = 
        do
            ignore <| isNotNull "name" name
            ignore <| isNotNull "argType" argType
            ignore <| isNotNull "mi" mi
        member __.Name with get() = name
        member __.ArgumentType with get() = argType
        member __.Invoke (arg: obj) (view:IView) : unit = mi.Invoke(view, [|arg|]) |> ignore
        interface IForestDescriptor with
            member __.Name = self.Name
        interface ICommandDescriptor with
            member __.ArgumentType = self.ArgumentType
            member __.Invoke arg view = self.Invoke arg view