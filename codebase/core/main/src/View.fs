namespace Forest

open Forest
open Forest.NullHandling

open System
open System.Collections.Generic
open System.Linq
open System.Text

type [<Interface>] IView<'T when 'T: (new: unit -> 'T)> =
    inherit IView
    abstract ViewModel: 'T with get, set

[<RequireQualifiedAccessAttribute>]
module View =
    type [<Sealed>] Path(region: string, index: int, view: string, parent: Path) =
        static member internal Separator = '/';
        static member internal IndexSuffix = '#';
        static member internal Empty = Path()
        new() = Path(String.Empty, -1, String.Empty)
        new(region: string, index: int, view: string) = Path(region, index, view, Path.Empty)

        override this.Equals o = StringComparer.Ordinal.Equals(this.ToString(), o.ToString())
        override this.GetHashCode () = this.ToString().GetHashCode()

        override this.ToString() = 
            let sb = StringBuilder().Append(parent).Append(Path.Separator)
            if index > 0 then sb.Append(index).Append(Path.IndexSuffix) |> ignore
            sb.Append(view).ToString()
            
        interface IEquatable<Path> with member this.Equals p = StringComparer.Ordinal.Equals(p.ToString(), this.ToString())
        interface IComparable<Path> with member this.CompareTo p = StringComparer.Ordinal.Compare(this.ToString(), p.ToString())

        member this.Parent with get() = parent
        member this.Region with get() = region
        member this.Index with get() = index
        member this.View with get() = view

    // TODO: argument verification
    type [<Sealed>] Descriptor(name: string, viewType: Type, viewModelType: Type, commands: IEnumerable<Command.Descriptor>) as self = 
        member this.Name with get() = name
        member this.ViewType with get() = viewType
        member this.ViewModelType with get() = viewModelType
        member this.Commands with get() = commands
        interface IViewDescriptor with
            member this.Name = self.Name
            member this.ViewType = self.ViewType
            member this.ViewModelType = self.ViewModelType
            member this.Commands = self.Commands.Cast<ICommandDescriptor>()

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

    [<Flags>]
    type internal Change =
        | ViewModel // of something

    type [<AbstractClass>] Base<'T when 'T: (new: unit -> 'T)> () as self =
        let mutable _viewModel : 'T = new 'T()
        let mutable _eventBus: IEventBus = Unchecked.defaultof<IEventBus>
        let mutable _instanceID: Guid = Guid.Empty

        let _viewChangeLog: ICollection<Change> = upcast LinkedList<Change>()

        member this.Publish<'M> (message: 'M, [<ParamArray>] topics: string[]) = 
            _eventBus.Publish(self, message, topics)

        member this.ViewModel
            with get ():'T = _viewModel
            and set (v: 'T) = _viewModel <- v
        member this.InstanceID
            with get() = _instanceID
            and set(v) = _instanceID <- v

        interface IViewInternal with
            member this.EventBus 
                with get() = _eventBus
                and set value = _eventBus <- (isNotNull "value" value)
            member this.InstanceID
                with get() = self.InstanceID
                and set v = self.InstanceID <- v

        interface IView<'T> with
            member this.ViewModel
                with get() = self.ViewModel
                and set v = 
                    self.ViewModel <- v
                    _viewChangeLog.Add Change.ViewModel

        interface IView with
            member this.Publish (m, t) : unit = self.Publish (m, t)
            member this.Regions with get() = raise (System.NotImplementedException())
            member this.ViewModel with get() = upcast self.ViewModel

