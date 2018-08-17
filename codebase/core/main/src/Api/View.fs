namespace Forest

open Forest

open System
open System.Reflection


type [<Interface>] IView<'T when 'T: (new: unit -> 'T)> =
    inherit IView
    abstract ViewModel: 'T with get

[<Serializable>]
type AbstractViewException(message: string, inner: Exception) =
    inherit Exception(isNotNull "message" message, inner)
    new (message: string) = AbstractViewException(message, null)

[<Serializable>]
type ViewAttributeMissingException(viewType: Type, inner: Exception) =
    inherit AbstractViewException(String.Format("The type `{0}` must be annotated with a `{1}`", viewType.FullName, typeof<ViewAttribute>.FullName), inner)
    new (viewType: Type) = ViewAttributeMissingException(isNotNull "viewType" viewType, null)

[<Serializable>]
type ViewTypeIsAbstractException(viewType: Type, inner: Exception) =
    inherit AbstractViewException(String.Format("Cannot instantiate view from type `{0}` because it is an interface or an abstract class.", (isNotNull "viewType" viewType).FullName), inner)
    new (viewType: Type) = ViewTypeIsAbstractException(isNotNull "viewType" viewType, null)

type [<AbstractClass>] AbstractView<'T when 'T: (new: unit -> 'T)> () as self =
    let mutable _viewModel : 'T = new 'T()
    let mutable _eventBus: IEventBus = nil<IEventBus>
    let mutable _instanceID: Identifier = Identifier.shell
    let mutable _viewStateModifier: IViewStateModifier = nil<IViewStateModifier>

    member __.Publish<'M> (message: 'M, [<ParamArray>] topics: string[]) = 
        _eventBus.Publish(self, message, topics)

    abstract member ResumeState: vm: 'T -> unit // TODO

    member __.FindRegion (NotNull "name" name) = upcast Region(name, self) : IRegion

    member __.ViewModel
        with get ():'T = _viewModel
        and set (NotNull "value" value: 'T) = (_viewModel <- value) |> _viewStateModifier.SetViewModel false _instanceID
    member __.InstanceID
        with get() = _instanceID
        and set(v) = _instanceID <- v

    interface IViewInternal with
        member __.ResumeState (NotNull "viewModel" viewModel) = self.ResumeState(downcast viewModel: 'T)
        member __.EventBus 
            with get() = _eventBus
            and set value = _eventBus <- value
        member __.InstanceID
            with get() = self.InstanceID
            and set v = self.InstanceID <- v
        member __.ViewStateModifier
            with get() = _viewStateModifier
            and set v = 
                match null2opt v with
                | Some md -> 
                    match md.GetViewModel self.InstanceID with
                    | Some vm -> _viewModel <- (downcast vm : 'T)
                    | None -> md.SetViewModel true self.InstanceID _viewModel
                | None -> 
                    match _viewStateModifier.GetViewModel self.InstanceID with
                    | Some vm -> _viewModel <- (downcast vm : 'T)
                    | None -> ()                    
                _viewStateModifier <- v

    interface IView<'T> with member __.ViewModel with get() = self.ViewModel

    interface IView with
        member __.Publish (m, t) : unit = self.Publish (m, t)
        member __.FindRegion name = self.FindRegion name
        member __.ViewModel with get() = upcast self.ViewModel

and private Region<'T when 'T: (new: unit -> 'T)>(regionName: string, view: AbstractView<'T>) as self =
    member __.ActivateView (NotNull "viewName" viewName: string) =
        (upcast view : IViewInternal).ViewStateModifier.ActivateView view.InstanceID regionName viewName

    member __.Name with get() = regionName

    interface IRegion with
        member __.Name = self.Name
        member __.ActivateView (viewName: string) = self.ActivateView viewName

[<RequireQualifiedAccessAttribute>]
module View =
    // TODO: argument verification
    type [<Sealed>] Descriptor internal (name: string, viewType: Type, viewModelType: Type, commands: Index<ICommandDescriptor, string>) as self = 
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

    let inline private _selectViewModelTypes (tt: Type) = 
        let isGenericView = tt.IsGenericType && (tt.GetGenericTypeDefinition() = typedefof<IView<_>>)
        match isGenericView with
        | true -> Some (tt.GetGenericArguments().[0])
        | false -> None

    let inline private tryGetViewModelType (t: Type) = 
        t.GetInterfaces()
        |> Seq.choose _selectViewModelTypes
        |> Seq.tryHead

    let getViewModelType (NotNull "viewType" viewType: Type) = Result.some (NonGenericView viewType) (tryGetViewModelType viewType)

    type [<Sealed>] Factory() as self = 
        member __.Resolve (NotNull "descriptor" descriptor : IViewDescriptor) : IView = 
            let flags = BindingFlags.Public|||BindingFlags.Instance
            let constructors = 
                descriptor.ViewType.GetConstructors(flags) 
                |> Array.toList
                |> List.filter (fun c -> c.GetParameters().Length = 0) 
            match constructors with
            | [] -> raise (InvalidOperationException(String.Format("View `{0}` does not have suitable constructor", descriptor.ViewType.FullName)))
            | head::_ -> downcast head.Invoke([||]) : IView
        
        interface IViewFactory with member __.Resolve m = self.Resolve m
