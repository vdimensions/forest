namespace Forest
open System
open System.Collections.Generic


[<AttributeUsage(AttributeTargets.Class)>]
[<Sealed>]
type ViewAttribute(name: string) = 
    inherit ForestNodeAttribute(name)
    member val AutowireCommands = false with get, set


[<RequireQualifiedAccessAttribute>]
module View = 
    let inline private isNotNull argName obj = match obj with | null -> nullArg argName | _ -> obj

    [<Sealed>]
    // TODO: argument verfication
    type Metadata(name: string, viewType: Type, commands: IEnumerable<Command.Metadata>) = 
        member this.Name with get() = name
        member this.ViewType with get() = viewType
        member this.Commands with get() = commands

    type Error = 
        | ViewAttributeMissing of Type
        | ViewTypeIsAbstract of Type

    type AbstractViewException(message: string, inner: exn) =
        inherit Exception(isNotNull "message" message, inner)
        new (message: string) = AbstractViewException(message, null)

    type ViewAttributeMissingException(viewType: Type, inner: exn) =
        inherit AbstractViewException(String.Format("View type {0} is an interface or abstract class.", (isNotNull "viewType" viewType).FullName), inner)
        new (viewType: Type) = ViewAttributeMissingException((isNotNull "viewType" viewType), null)

