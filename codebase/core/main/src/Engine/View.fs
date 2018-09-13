namespace Forest

open Forest
open Forest.Collections
open Forest.NullHandling

open System
open System.Collections.Generic
open System.Reflection
open System.Diagnostics


type [<AbstractClass;NoComparison>] AbstractView<'T>(vm:'T) =
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private hierarchyKey:TreeNode
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private rt:IForestRuntime
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private descriptor:IViewDescriptor
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable vm:'T = vm
    new() = AbstractView(downcast Activator.CreateInstance(typeof<'T>))
    member this.Publish<'M> (message: 'M, [<ParamArray>] topics: string[]) = 
        this.rt.PublishEvent this message topics
    abstract member Load: unit -> unit
    abstract member Resume: unit -> unit
    default __.Resume() = ()
    member this.FindRegion (NotNull "name" name) = 
        upcast Region(name, this) : IRegion
    member this.ViewModel
        with get ():'T = vm
         and set (NotNull "value" value: 'T) = 
            vm <- 
                match null2vopt this.rt with
                // The default behaviour
                | ValueSome rt -> (rt.SetViewModel false this.hierarchyKey value)
                // This case is entered if the view model is set at construction time, for example, by a DI container.
                | ValueNone -> value
    member internal this.HierarchyKey
        with get() = this.hierarchyKey
    interface IRuntimeView with
        member this.Load () = this.Load()
        member this.Resume viewModel =
            vm <- this.rt.SetViewModel true this.hierarchyKey (downcast viewModel : 'T)
            this.Resume()
        member this.InstanceID
            with get() = this.hierarchyKey
        member this.Descriptor
            with get() = this.descriptor
        member this.Runtime
            with get() = this.rt
        member this.AcquireRuntime (node:TreeNode) (vd:IViewDescriptor) (NotNull "runtime" runtime:IForestRuntime) =
            match null2vopt this.rt with
            | ValueNone ->
                this.descriptor <- vd
                this.hierarchyKey <- node
                match runtime.GetViewModel this.HierarchyKey with
                | Some viewModelFromState -> vm <- (downcast viewModelFromState : 'T)
                | None -> ignore <| runtime.SetViewModel true this.HierarchyKey vm
                runtime.SubscribeEvents this
                this.rt <- runtime
                ()
            | ValueSome _ -> raise (InvalidOperationException(String.Format("View {0} is already within a modification scope", this.hierarchyKey.View)))
        member this.AbandonRuntime (_) =
            match null2vopt this.rt with
            | ValueSome currentModifier ->
                currentModifier.UnsubscribeEvents this
                match currentModifier.GetViewModel this.HierarchyKey with
                | Some viewModelFromState -> 
                    vm <- (downcast viewModelFromState : 'T)
                    ()
                | None -> () 
                this.rt <- nil<IForestRuntime>
            | ValueNone -> ()
    interface IView<'T> with
        member this.ViewModel
            with get() = this.ViewModel
             and set v = this.ViewModel <- v
    interface IView with
        member this.Publish (m, t) = this.Publish (m, t)
        member this.FindRegion name = this.FindRegion name
        member this.ViewModel with get() = upcast this.ViewModel

 and private Region(regionName:string, view:IRuntimeView) =
    member __.ActivateView (NotNull "viewName" viewName:string) =
        view.Runtime.ActivateView view.InstanceID regionName viewName
    member __.ActivateView<'v when 'v:>IView> () : 'v =
        view.Runtime.ActivateAnonymousView view.InstanceID regionName
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
    type [<Sealed;NoComparison>] internal Descriptor internal (name:vname, viewType:Type, viewModelType:Type, commands:Index<ICommandDescriptor, cname>, events:IEventDescriptor array) = 
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

    type [<Struct;NoComparison>] Error =
        | ViewAttributeMissing of nonAnnotatedViewType:Type
        | ViewTypeIsAbstract of abstractViewType:Type
        | NonGenericView of nonGenericViewType:Type

    #if NETSTANDARD
    let inline private _selectViewModelTypes (tt:TypeInfo) =
    #else
    let inline private _selectViewModelTypes (tt:Type) =
    #endif
        let isGenericView = tt.IsGenericType && (tt.GetGenericTypeDefinition() = typedefof<IView<_>>)
        match isGenericView with
        | true -> Some (tt.GetGenericArguments().[0])
        | false -> None
    let inline private _tryGetViewModelType (t:Type) = 
        t.GetInterfaces()
        #if NETSTANDARD
        |> Seq.map (fun t -> t.GetTypeInfo())
        #endif
        |> Seq.choose _selectViewModelTypes
        |> Seq.tryHead
    let getViewModelType (NotNull "viewType" viewType:Type) = Result.some (NonGenericView viewType) (_tryGetViewModelType viewType)

    let inline resolveError (e:Error) =
        match e with
        | NonGenericView vt -> raise <| ViewTypeIsNotGenericException vt
        | _ -> ()

    type [<Sealed;NoComparison>] Factory() = 
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
