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
    let mutable _viewModel:'T = new 'T()
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable _hkey:HierarchyKey = HierarchyKey.shell
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable _runtime:IForestRuntime = nil<IForestRuntime>
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable _descriptor:IViewDescriptor = nil<IViewDescriptor>

    member this.Publish<'M> (message: 'M, [<ParamArray>] topics: string[]) = 
        _runtime.PublishEvent this message topics
    abstract member Load: unit -> unit
    abstract member Resume: unit -> unit
    default __.Resume() = ()
    member this.FindRegion (NotNull "name" name) = 
        upcast Region(name, this) : IRegion
    member __.ViewModel
        with get ():'T = _viewModel
         and set (NotNull "value" value: 'T) = _viewModel <- (_runtime.SetViewModel false _hkey value)
    member internal __.HierarchyKey
        with get() = _hkey
         and set(NotNull "value" value) = _hkey <- value
    interface IViewState with
        member this.Load () = this.Load()
        member this.Resume viewModel =
            _viewModel <- _runtime.SetViewModel true _hkey (downcast viewModel : 'T)
            this.Resume()
        member this.InstanceID
            with get() = this.HierarchyKey
             and set value = this.HierarchyKey <- value
        member __.Descriptor
            with get() = _descriptor
             and set v = _descriptor <- v
        member __.Runtime
            with get() = _runtime
        member this.AcquireRuntime (NotNull "runtime" runtime:IForestRuntime) =
            match null2vopt _runtime with
            | ValueNone ->
                match runtime.GetViewModel this.HierarchyKey with
                | Some viewModelFromState -> _viewModel <- (downcast viewModelFromState : 'T)
                | None -> ignore <| runtime.SetViewModel true this.HierarchyKey _viewModel
                runtime.SubscribeEvents this
                _runtime <- runtime
                ()
            | ValueSome _ -> raise (InvalidOperationException(String.Format("View {0} is already within a modification scope", _hkey.View)))
        member this.AbandonRuntime (_) =
            match null2vopt _runtime with
            | ValueSome currentModifier ->
                currentModifier.UnsubscribeEvents this
                match currentModifier.GetViewModel this.HierarchyKey with
                | Some viewModelFromState -> 
                    _viewModel <- (downcast viewModelFromState : 'T)
                    ()
                | None -> () 
                _runtime <- nil<IForestRuntime>
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
        (upcast view:IViewState).Runtime.ActivateView view.HierarchyKey regionName viewName
    member __.ActivateView<'v when 'v:>IView> () : 'v =
        (upcast view:IViewState).Runtime.ActivateAnonymousView view.HierarchyKey regionName
    member __.Name 
        with get() = regionName
    interface IRegion with
        member this.Name = this.Name
        member this.ActivateView (viewName: string) = this.ActivateView viewName
        member this.ActivateView<'v when 'v:>IView>() = this.ActivateView<'v>()

[<RequireQualifiedAccessAttribute>]
[<CompiledName("View")>]
module View =
    // TODO: argument verification
    type [<Sealed>] internal Descriptor internal (name:vname, viewType:Type, viewModelType:Type, commands:Index<ICommandDescriptor, cname>, events:IEventDescriptor array) = 
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
