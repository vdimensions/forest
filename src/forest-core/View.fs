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
type IViewDescriptor = 
    abstract Name: string with get
    abstract ViewType: Type with get
    abstract ViewModelType: Type with get
    abstract Commands: IEnumerable<ICommandMetadata> with get

[<RequireQualifiedAccessAttribute>]
module View = 
    let inline private isNotNull argName obj = match obj with | null -> nullArg argName | _ -> obj

    [<Sealed>]
    // TODO: argument verfication
    type Descriptor(name: string, viewType: Type, viewModelType: Type, commands: IEnumerable<Command.Metadata>) as self = 
        member this.Name with get() = name
        member this.ViewType with get() = viewType
        member this.ViewModelType with get() = viewModelType
        member this.Commands with get() = commands
        interface IViewDescriptor with
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
        inherit AbstractViewException(String.Format("The type `{0}` must be annotated with a `{1}`", viewType.FullName, typeof<ViewAttribute>.FullName), inner)
        new (viewType: Type) = ViewAttributeMissingException((isNotNull "viewType" viewType), null)

    type ViewTypeIsAbstractException(viewType: Type, inner: exn) =
        inherit AbstractViewException(String.Format("Cannot instantiate view from type `{0}` because it is an interface or an abstract class.", (isNotNull "viewType" viewType).FullName), inner)
        new (viewType: Type) = ViewTypeIsAbstractException((isNotNull "viewType" viewType), null)

