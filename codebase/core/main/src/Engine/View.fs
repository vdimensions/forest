namespace Forest

open Forest
open Forest.Collections
open Forest.NullHandling

open System
open System.Collections.Generic
open System.Reflection
open System.Diagnostics


type [<AbstractClass>] AbstractView<'T when 'T: (new: unit -> 'T)> () =
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable _viewModel : 'T = new 'T()
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable _hkey: HierarchyKey = HierarchyKey.shell
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable _viewStateModifier: IViewStateModifier = nil<IViewStateModifier>
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable _descriptor: IViewDescriptor = nil<IViewDescriptor>

    member this.Publish<'M> (message: 'M, [<ParamArray>] topics: string[]) = 
        _viewStateModifier.PublishEvent this message topics

    abstract member Load: unit -> unit
    abstract member Resume: unit -> unit
    default __.Resume() = ()

    member this.FindRegion (NotNull "name" name) = 
        upcast Region(name, this) : IRegion

    member __.ViewModel
        with get ():'T = _viewModel
         and set (NotNull "value" value: 'T) = _viewModel <- (_viewStateModifier.SetViewModel false _hkey value)
    member internal __.HierarchyKey
        with get() = _hkey
         and set(NotNull "value" value) = _hkey <- value

    interface IViewState with
        member this.Load () = this.Load()
        member this.Resume viewModel =
            _viewModel <- _viewStateModifier.SetViewModel true _hkey (downcast viewModel : 'T)
            this.Resume()
        member this.InstanceID
            with get() = this.HierarchyKey
             and set value = this.HierarchyKey <- value
        member __.Descriptor
            with get() = _descriptor
             and set v = _descriptor <- v
        member __.ViewStateModifier
            with get() = _viewStateModifier
        member this.EnterModificationScope (NotNull "modifier" modifier: IViewStateModifier) =
            match null2vopt _viewStateModifier with
            | ValueNone ->
                match modifier.GetViewModel this.HierarchyKey with
                | Some viewModelFromState -> _viewModel <- (downcast viewModelFromState : 'T)
                | None -> ignore <| modifier.SetViewModel true this.HierarchyKey _viewModel
                modifier.SubscribeEvents this
                _viewStateModifier <- modifier
                ()
            | ValueSome _ -> raise (InvalidOperationException(String.Format("View {0} is already within a modification scope", _hkey.View)))

        member this.LeaveModificationScope (_) =
            match null2vopt _viewStateModifier with
            | ValueSome currentModifier ->
                currentModifier.UnsubscribeEvents this
                match currentModifier.GetViewModel this.HierarchyKey with
                | Some viewModelFromState -> 
                    _viewModel <- (downcast viewModelFromState : 'T)
                    ()
                | None -> () 
                _viewStateModifier <- nil<IViewStateModifier>
            | ValueNone -> ()

    interface IView<'T> with 
        member this.ViewModel
            with get() = this.ViewModel
             and set v = this.ViewModel <- v

    interface IView with
        member this.Publish (m, t) = this.Publish (m, t)
        member this.FindRegion name = this.FindRegion name
        member this.ViewModel with get() = upcast this.ViewModel

and private Region<'T when 'T: (new: unit -> 'T)>(regionName:string, view:AbstractView<'T>) =
    member __.ActivateView (NotNull "viewName" viewName:string) =
        (upcast view:IViewState).ViewStateModifier.ActivateView view.HierarchyKey regionName viewName

    member __.Name 
        with get() = regionName

    interface IRegion with
        member this.Name = this.Name
        member this.ActivateView (viewName: string) = this.ActivateView viewName

[<RequireQualifiedAccessAttribute>]
module View =
    // TODO: argument verification
    type [<Sealed>] internal Descriptor internal (name:string, viewType:Type, viewModelType:Type, commands:Index<ICommandDescriptor, string>, events:IEventDescriptor array) = 
        member __.Name with get() = name
        member __.ViewType with get() = viewType
        member __.ViewModelType with get() = viewModelType
        member __.Commands with get() = commands
        member __.Events with get() = upcast events:IEnumerable<IEventDescriptor>
        interface IViewDescriptor with
            member this.Name = this.Name
            member this.ViewType = this.ViewType
            member this.ViewModelType = this.ViewModelType
            member this.Commands = this.Commands
            member this.Events = this.Events

    type [<Struct>] Error = 
        | ViewAttributeMissing of nonAnnotatedViewType:Type
        | ViewTypeIsAbstract of abstractViewType:Type
        | NonGenericView of nonGenericViewType:Type

    let inline private _selectViewModelTypes (tt:Type) = 
        let isGenericView = tt.IsGenericType && (tt.GetGenericTypeDefinition() = typedefof<IView<_>>)
        match isGenericView with
        | true -> Some (tt.GetGenericArguments().[0])
        | false -> None

    let inline private _tryGetViewModelType (t:Type) = 
        t.GetInterfaces()
        |> Seq.choose _selectViewModelTypes
        |> Seq.tryHead

    let getViewModelType (NotNull "viewType" viewType:Type) = Result.some (NonGenericView viewType) (_tryGetViewModelType viewType)

    type [<Sealed>] Factory() = 
        member __.Resolve (NotNull "descriptor" descriptor:IViewDescriptor) : IView = 
            let flags = BindingFlags.Public|||BindingFlags.Instance
            let constructors = 
                descriptor.ViewType.GetConstructors(flags) 
                |> Array.toList
                |> List.filter (fun c -> c.GetParameters().Length = 0) 
            match constructors with
            | [] -> raise (InvalidOperationException(String.Format("View `{0}` does not have suitable constructor", descriptor.ViewType.FullName)))
            | head::_ -> downcast head.Invoke([||]) : IView
        
        interface IViewFactory with member this.Resolve m = this.Resolve m
