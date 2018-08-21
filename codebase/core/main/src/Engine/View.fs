namespace Forest

open Forest

open System
open System.Reflection


// contains the active mutable forest state, such as the latest dom index and view state changes
type [<Sealed>] internal ViewState(id: Identifier, descriptor: IViewDescriptor, viewInstance: IViewInternal) =
    member __.ID with get() = id
    member __.Descriptor with get() = descriptor
    member __.View with get() = viewInstance

type [<AbstractClass>] AbstractView<'T when 'T: (new: unit -> 'T)> () as self =
    let mutable _viewModel : 'T = new 'T()
    let mutable _eventBus: IEventBus = nil<IEventBus>
    let mutable _instanceID: Identifier = Identifier.shell
    let mutable _viewStateModifier: IViewStateModifier = nil<IViewStateModifier>

    member __.Publish<'M> (message: 'M, [<ParamArray>] topics: string[]) = 
        _eventBus.Publish(self, message, topics)

    abstract member ResumeState: unit -> unit // TODO

    member __.FindRegion (NotNull "name" name) = upcast Region(name, self) : IRegion

    member __.ViewModel
        with get ():'T = _viewModel
        and set (NotNull "value" value: 'T) = (_viewModel <- value) |> _viewStateModifier.SetViewModel false _instanceID
    member internal __.InstanceID
        with get() = _instanceID
        and set(NotNull "value" value) = _instanceID <- value

    interface IViewInternal with
        member __.ResumeState () = self.ResumeState()
        member __.EventBus 
            with get() = _eventBus
            and set value = _eventBus <- value
        member __.InstanceID
            with get() = self.InstanceID
            and set value = self.InstanceID <- value
        member __.ViewStateModifier
            with get() = _viewStateModifier
            and set value = 
                match null2opt value with
                | Some md -> 
                    match md.GetViewModel self.InstanceID with
                    | Some vm -> _viewModel <- (downcast vm : 'T)
                    | None -> md.SetViewModel true self.InstanceID _viewModel
                | None -> 
                    match _viewStateModifier |> (null2opt >>= (fun vsm -> vsm.GetViewModel self.InstanceID)) with
                    | Some vm -> _viewModel <- (downcast vm : 'T)
                    | None -> ()                    
                _viewStateModifier <- value

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
    type [<Sealed>] internal Descriptor internal (name: string, viewType: Type, viewModelType: Type, commands: Index<ICommandDescriptor, string>) as self = 
        member __.Name with get() = name
        member __.ViewType with get() = viewType
        member __.ViewModelType with get() = viewModelType
        member __.Commands with get() = commands
        interface IForestDescriptor with
            member __.Name = self.Name
        interface IViewDescriptor with
            member __.ViewType = self.ViewType
            member __.ViewModelType = self.ViewModelType
            member __.Commands = self.Commands

    type [<Struct>] Error = 
        | ViewAttributeMissing of NonAnnotatedViewType: Type
        | ViewTypeIsAbstract of AbstractViewType: Type
        | NonGenericView of NonGenericViewType: Type

    let inline private _selectViewModelTypes (tt: Type) = 
        let isGenericView = tt.IsGenericType && (tt.GetGenericTypeDefinition() = typedefof<IView<_>>)
        match isGenericView with
        | true -> Some (tt.GetGenericArguments().[0])
        | false -> None

    let inline private _tryGetViewModelType (t: Type) = 
        t.GetInterfaces()
        |> Seq.choose _selectViewModelTypes
        |> Seq.tryHead

    let getViewModelType (NotNull "viewType" viewType: Type) = Result.some (NonGenericView viewType) (_tryGetViewModelType viewType)

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
