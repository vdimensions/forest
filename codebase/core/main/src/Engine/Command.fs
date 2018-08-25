namespace Forest

open Forest.NullHandling
open Forest.Reflection

open System

[<RequireQualifiedAccess>]
module Command = 

    type [<Struct>] Error =
        | NonVoidReturnType of methodWithReturnValue: ICommandMethod
        | MoreThanOneArgument of multiArgumentMethod: ICommandMethod
        | MultipleErrors of errors: Error list

    // TODO: argument verification
    type [<Sealed>] internal Descriptor(argType: Type, mi: ICommandMethod) = 
        do
            ignore <| isNotNull "argType" argType
            ignore <| isNotNull "mi" mi
        member __.ArgumentType with get() = argType
        member __.Invoke (arg: obj) (view:IView) : unit = mi.Invoke(view, [|arg|]) |> ignore
        interface ICommandDescriptor with
            member this.Name = mi.Name
            member this.ArgumentType = this.ArgumentType
            member this.Invoke arg view = this.Invoke arg view