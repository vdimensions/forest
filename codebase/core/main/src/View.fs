namespace Forest

open Forest

open System


type [<Interface>] IView<'T when 'T: (new: unit -> 'T)> =
    inherit IView
    abstract ViewModel: 'T with get, set

type AbstractViewException(message: string, inner: Exception) =
    inherit Exception(isNotNull "message" message, inner)
    new (message: string) = AbstractViewException(message, null)

type ViewAttributeMissingException(viewType: Type, inner: Exception) =
    inherit AbstractViewException(String.Format("The type `{0}` must be annotated with a `{1}`", viewType.FullName, typeof<ViewAttribute>.FullName), inner)
    new (viewType: Type) = ViewAttributeMissingException((isNotNull "viewType" viewType), null)

type ViewTypeIsAbstractException(viewType: Type, inner: Exception) =
    inherit AbstractViewException(String.Format("Cannot instantiate view from type `{0}` because it is an interface or an abstract class.", (isNotNull "viewType" viewType).FullName), inner)
    new (viewType: Type) = ViewTypeIsAbstractException((isNotNull "viewType" viewType), null)

[<RequireQualifiedAccessAttribute>]
module View =
    // TODO: argument verification
    type [<Sealed>] Descriptor(name: string, viewType: Type, viewModelType: Type, commands: IIndex<ICommandDescriptor, string>) as self = 
        member __.Name with get() = name
        member __.ViewType with get() = viewType
        member __.ViewModelType with get() = viewModelType
        member __.Commands with get() = commands
        interface IViewDescriptor with
            member __.Name = self.Name
            member __.ViewType = self.ViewType
            member __.ViewModelType = self.ViewModelType
            member __.Commands = self.Commands

    type Error = 
        | ViewAttributeMissing of Type
        | ViewTypeIsAbstract of Type
        | NonGenericView of Type

    type [<AbstractClass>] Base<'T when 'T: (new: unit -> 'T)> () as self =
        let mutable _viewModel : 'T = new 'T()
        let mutable _eventBus: IEventBus = Unchecked.defaultof<IEventBus>
        let mutable _instanceID: Guid = Guid.Empty

        member __.Publish<'M> (message: 'M, [<ParamArray>] topics: string[]) = 
            _eventBus.Publish(self, message, topics)

        member __.ViewModel
            with get ():'T = _viewModel
            and set (v: 'T) = _viewModel <- v
        member __.InstanceID
            with get() = _instanceID
            and set(v) = _instanceID <- v

        interface IViewInternal with
            member __.EventBus 
                with get() = _eventBus
                and set value = _eventBus <- (isNotNull "value" value)
            member __.InstanceID
                with get() = self.InstanceID
                and set v = self.InstanceID <- v

        interface IView<'T> with
            member __.ViewModel
                with get() = self.ViewModel
                and set v = self.ViewModel <- v

        interface IView with
            member __.Publish (m, t) : unit = self.Publish (m, t)
            member __.Regions with get() = raise (System.NotImplementedException())
            member __.ViewModel with get() = upcast self.ViewModel

