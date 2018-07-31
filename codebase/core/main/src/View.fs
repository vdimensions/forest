namespace Forest

open Forest.NullHandling

open System
open System.Collections.Generic
open System.Linq

type [<Interface>] IView<'T when 'T: (new: unit -> 'T)> =
    inherit IView
    abstract ViewModel: 'T with get, set

[<RequireQualifiedAccessAttribute>]
module View = 

    // internal functionality needed by the forest engine
    type [<Interface>] internal IViewInternal =
        inherit IView
        /// <summary>
        /// Submits the current view state to the specified <see cref="IForestContext"/> instance.
        /// </summary>
        /// <param name="context">
        /// The <see cref="IForestRuntime" /> instance to manage the state of the current view.
        /// </param>
        abstract member Submit: ctx: IForestContext -> unit

        abstract EventBus: IEventBus with get, set

    type Path(name: string) =
        member this.Name with get() = name

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

    type [<AbstractClass>] Base<'T when 'T: (new: unit -> 'T)> () as self =
        let mutable _viewModel : 'T  = new 'T()
        let mutable _eventBus: IEventBus = Unchecked.defaultof<IEventBus>
        let _viewChangeLog: ICollection<ViewChange> = upcast LinkedList<ViewChange>()
        member this.Publish<'M> (message: 'M, [<ParamArray>] topics: string[]) = 
            _eventBus.Publish(self, message, topics)
        member this.ViewModel
            with get ():'T = _viewModel
            and set (v: 'T) = _viewModel <- v

        interface IViewInternal with
            member this.Submit rt =
               ()
            member this.EventBus 
                with get() = _eventBus
                and set value = _eventBus <- (isNotNull "value" value)
        interface IView<'T> with
            member this.ViewModel
                with get() = self.ViewModel
                and set v = 
                    self.ViewModel <- v
                    _viewChangeLog.Add ViewChange.ViewModel
        interface IView with
            member this.Publish (m, t) : unit = self.Publish (m, t)
            member this.Regions with get() = raise (System.NotImplementedException())
            member this.ViewModel with get() = upcast self.ViewModel

