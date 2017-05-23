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
    [<Sealed>]
    type Metadata(name: string, viewType: Type, commands: Command.Metadata[]) = 
        member this.Name with get() = name
        member this.ViewType with get() = viewType
        member this.Commands with get() = upcast commands: IEnumerable<Command.Metadata>

    type Error = 
    | ViewAttributeMissing of Type
    | ViewTypeIsAbstract of Type

    type AbstractViewException(message: string) =
        inherit Exception(message)

    type ViewAttributeMissingException(viewType: Type) =
        inherit AbstractViewException(
            String.Format("View type {0} is an interface or abstract class.", viewType.FullName))
        

