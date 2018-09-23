namespace Forest

open Forest.NullHandling
open Forest.Reflection

open System

[<RequireQualifiedAccess>]
[<CompiledName("Command")>]
module Command = 
    type [<Struct;NoComparison>] Error =
        | CommandNotFound of owner:Type * command:cname
        | InvocationError of cause:exn
        | NonVoidReturnType of methodWithReturnValue:ICommandMethod
        | MoreThanOneArgument of multiArgumentMethod:ICommandMethod
        | MultipleErrors of errors:Error list

    // TODO: argument verification
    type [<Sealed;NoComparison>] internal Descriptor(argType:Type, method:ICommandMethod) = 
        do
            ignore <| isNotNull "argType" argType
            ignore <| isNotNull "mi" method
        member __.Invoke (arg:obj) (view:IView) : unit = method.Invoke view arg
        member __.ArgumentType with get() = argType
        interface ICommandDescriptor with
            member __.Name = method.CommandName
            member this.ArgumentType = this.ArgumentType
            member this.Invoke arg view = this.Invoke arg view

    [<CompiledName("CommandModel")>]
    type [<Sealed;NoComparison>] internal Model(name:cname, displayName:string, tooltip:string, description:string) =
        new (name:cname) = Model(name, String.Empty, String.Empty, String.Empty)
        member __.Name with get() = name
        member val DisplayName = displayName with get, set
        member val Tooltip = tooltip with get, set
        member val Description = description with get, set
        interface ICommandModel with
            member this.Name = this.Name
            member this.DisplayName = this.DisplayName
            member this.Tooltip = this.Tooltip
            member this.Description = this.Description

    let resolveError (e:Error) =
        // TODO
        ()
