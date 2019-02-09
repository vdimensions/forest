namespace Forest
open System
open Axle.Verification
open Forest.Reflection

[<RequireQualifiedAccess>]
[<CompiledName("Command")>]
module Command = 
    [<NoComparison>] 
    type Error =
        | CommandNotFound of owner : Type * command : cname
        | InvocationError of owner : Type * command : cname * cause : exn
        | NonVoidReturnType of commandMethod : ICommandMethod
        | MoreThanOneArgument of commandMethod : ICommandMethod
        | MultipleErrors of errors : Error list

    [<Sealed;NoComparison>] 
    type internal Descriptor(argType : Type, method : ICommandMethod) = 
        do
            ignore <| (|NotNull|) "argType" argType
            ignore <| (|NotNull|) "mi" method
        member __.Invoke (arg : obj) (view : IView) : unit = method.Invoke view arg
        member __.ArgumentType with get() = argType
        interface ICommandDescriptor with
            member __.Name = method.CommandName
            member this.ArgumentType = this.ArgumentType
            member this.Invoke arg view = this.Invoke arg view

    [<CompiledName("CommandModel")>]
    type [<Sealed;NoComparison>] internal Model(name : cname, displayName : string, tooltip : string, description : string) =
        new (name : cname) = Model(name, String.Empty, String.Empty, String.Empty)
        member __.Name with get() = name
        member val DisplayName = displayName with get, set
        member val Tooltip = tooltip with get, set
        member val Description = description with get, set
        interface ICommandModel with
            member this.Name = this.Name
            member this.DisplayName = this.DisplayName
            member this.Tooltip = this.Tooltip
            member this.Description = this.Description

    let resolveError = function
        | MoreThanOneArgument mi -> upcast InvalidOperationException() : exn
        | NonVoidReturnType mi -> upcast InvalidOperationException() : exn
        | CommandNotFound (o, c) -> upcast InvalidOperationException() : exn
        | InvocationError (o, c, e) -> upcast InvalidOperationException() : exn
        | MultipleErrors e -> upcast InvalidOperationException() : exn
    let handleError (e : Error) = e |> resolveError |> raise
