﻿namespace Forest

open Forest.NullHandling
open Forest.Reflection

open System

[<RequireQualifiedAccess>]
[<CompiledName("Command")>]
module Command = 
    type [<Struct>] Error =
        | CommandNotFound of owner:Type * command:cname
        | InvocationError of cause:exn
        | NonVoidReturnType of methodWithReturnValue:ICommandMethod
        | MoreThanOneArgument of multiArgumentMethod:ICommandMethod
        | MultipleErrors of errors:Error list

    // TODO: argument verification
    type [<Sealed>] internal Descriptor(argType:Type, method:ICommandMethod) = 
        do
            ignore <| isNotNull "argType" argType
            ignore <| isNotNull "mi" method
        member __.ArgumentType with get() = argType
        member __.Invoke (arg:obj) (view:IView) : unit = method.Invoke(view, [|arg|]) |> ignore
        interface ICommandDescriptor with
            member __.Name = method.Name
            member this.ArgumentType = this.ArgumentType
            member this.Invoke arg view = this.Invoke arg view

    [<CompiledName("CommandModel")>]
    type Model(name:cname, tooltip:string, description:string) =
        member this.Name with get() = name
        member val Tooltip = tooltip with get, set
        member val Description = description with get, set
