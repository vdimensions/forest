namespace Forest
open System
open System.Collections.Generic
open System.Linq


[<AttributeUsage(AttributeTargets.Class)>]
[<Sealed>]
type ViewAttribute(name: string) = 
    inherit ForestNodeAttribute(name)
    member val AutowireCommands = false with get, set

[<Interface>]
type IViewMetadata = 
    abstract Name: string with get
    abstract ViewType: Type with get
    abstract ViewModelType: Type with get
    abstract Commands: IEnumerable<ICommandMetadata> with get

[<RequireQualifiedAccessAttribute>]
module View = 
    let inline private isNotNull argName obj = match obj with | null -> nullArg argName | _ -> obj

    [<Sealed>]
    // TODO: argument verfication
    type Metadata(name: string, viewType: Type, viewModelType: Type, commands: IEnumerable<Command.Metadata>) as self = 
        member this.Name with get() = name
        member this.ViewType with get() = viewType
        member this.ViewModelType with get() = viewModelType
        member this.Commands with get() = commands
        interface IViewMetadata with
            member this.Name = self.Name
            member this.ViewType = self.ViewType
            member this.ViewModelType = self.ViewModelType
            member this.Commands = self.Commands.Cast<ICommandMetadata>()

    type Error = 
        | ViewAttributeMissing of Type
        | ViewTypeIsAbstract of Type
        | NonGenericView of Type

    type AbstractViewException(message: string, inner: exn) =
        inherit Exception(isNotNull "message" message, inner)
        new (message: string) = AbstractViewException(message, null)

    type ViewAttributeMissingException(viewType: Type, inner: exn) =
        inherit AbstractViewException(String.Format("View type {0} is an interface or abstract class.", (isNotNull "viewType" viewType).FullName), inner)
        new (viewType: Type) = ViewAttributeMissingException((isNotNull "viewType" viewType), null)

