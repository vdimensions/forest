namespace Forest

open Forest
open Forest.Collections
open Forest.NullHandling

open System
open System.Diagnostics
open System.Reflection


type [<AbstractClass;NoComparison>] AbstractView<[<EqualityConditionalOn>]'T>(vm : 'T) =
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private hierarchyKey : TreeNode
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private rt : IForestRuntime
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private descriptor : IViewDescriptor
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable vm : 'T = vm

    new() = AbstractView(downcast Activator.CreateInstance(typeof<'T>))

    override this.Finalize() =
        this.Dispose(false)

    member this.Publish<'M> (message : 'M, [<ParamArray>] topics : string[]) = 
        this.rt.PublishEvent this message topics

    abstract member Load : unit -> unit
    default __.Load() = ()

    abstract member Resume : unit -> unit
    default __.Resume() = ()

    abstract member Dispose : bool -> unit
    default __.Dispose(_) = ()

    member this.FindRegion (NotNull "name" name) = 
        upcast Region(name, this) : IRegion

    member this.Close() =
        match null2vopt this.rt with
        | ValueNone ->
            invalidOp("No runtime available!")
        | ValueSome rt -> 
            let (parent, region) = (this.hierarchyKey.Parent, this.hierarchyKey.Region)
            rt.RemoveViewFromRegion parent region (System.Predicate(fun v -> obj.ReferenceEquals(v, this)))

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
        member this.Load () = 
            this.Load()

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
            | ValueSome _ -> invalidOp(String.Format("View {0} is already captured by a runtime", this.hierarchyKey.View))
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
        member this.Close() = this.Close()
        member this.ViewModel with get() = upcast this.ViewModel

    interface IDisposable with
        member this.Dispose() =
            this.Dispose(true)
            this |> GC.SuppressFinalize

 and private Region(regionName:rname, owner:IRuntimeView) =
    member __.ActivateView (NotNull "viewName" viewName:vname) =
        owner.Runtime.ActivateView(viewName, regionName, owner.InstanceID)

    member __.ActivateView (NotNull "viewName" viewName:vname, NotNull "model" model:'m) =
        owner.Runtime.ActivateView(model, viewName, regionName, owner.InstanceID)

    member __.ActivateView<'v when 'v:>IView> () : 'v =
        owner.Runtime.ActivateAnonymousView(regionName, owner.InstanceID)

    member __.ActivateView<'v, 'm when 'v:>IView<'m>> (model:'m) : 'v =
        owner.Runtime.ActivateAnonymousView(model, regionName, owner.InstanceID)

    member this.Clear() =
        owner.Runtime.ClearRegion owner.InstanceID regionName
        this

    member this.Remove(NotNull "predicate" predicate : System.Predicate<IView>) =
        owner.Runtime.RemoveViewFromRegion owner.InstanceID regionName predicate
        this

    member __.GetContents() =
        owner.Runtime.GetRegionContents owner.InstanceID regionName

    member __.Name 
        with get() = regionName

    interface IRegion with
        member this.ActivateView (viewName : vname) = this.ActivateView viewName
        member this.ActivateView<'v when 'v :> IView>() = this.ActivateView<'v>()
        member this.ActivateView<'m> (viewName : vname, model : 'm) = this.ActivateView<'m>(viewName, model)
        member this.ActivateView<'v, 'm when 'v :> IView<'m>> (model : 'm) = this.ActivateView<'v, 'm>(model)
        member this.Clear() = upcast this.Clear()
        member this.Remove predicate = upcast this.Remove(predicate)
        member this.Name = this.Name
        member this.Views = this.GetContents()

type [<AbstractClass>] AbstractView() = inherit AbstractView<Unit>(())

[<RequireQualifiedAccessAttribute>]
[<CompiledName("View")>]
module View =
    // TODO: argument verification
    type [<Sealed;NoComparison>] internal Descriptor internal (name : vname, viewType : Type, viewModelType : Type, commands : Index<ICommandDescriptor, cname>, events : IEventDescriptor array) = 
        member __.Name with get() = name
        member __.ViewType with get() = viewType
        member __.ViewModelType with get() = viewModelType
        member __.Commands with get() = commands
        member __.Events with get() = upcast events : IEventDescriptor seq
        interface IViewDescriptor with
            member this.Name = this.Name
            member this.ViewType = this.ViewType
            member this.ViewModelType = this.ViewModelType
            member this.Commands = this.Commands
            member this.Events = this.Events

    type [<Struct;NoComparison>] Error =
        | ViewAttributeMissing of nonAnnotatedViewType : Type
        | ViewTypeIsAbstract of abstractViewType : Type
        | NonGenericView of nonGenericViewType : Type

    #if NETSTANDARD
    let inline private _selectViewModelTypes (tt : TypeInfo) =
    #else
    let inline private _selectViewModelTypes (tt : Type) =
    #endif
        let isGenericView = tt.IsGenericType && (tt.GetGenericTypeDefinition() = typedefof<IView<_>>)
        match isGenericView with
        | true -> Some (tt.GetGenericArguments().[0])
        | false -> None

    let inline private _tryGetViewModelType (t : Type) = 
        t.GetInterfaces()
        #if NETSTANDARD
        |> Seq.map (fun t -> t.GetTypeInfo())
        #endif
        |> Seq.choose _selectViewModelTypes
        |> Seq.tryHead

    let getViewModelType (NotNull "viewType" viewType : Type) = Result.some (NonGenericView viewType) (_tryGetViewModelType viewType)

    let inline resolveError (e : Error) =
        match e with
        | NonGenericView vt -> raise <| ViewTypeIsNotGenericException vt
        | _ -> ()

    type [<Sealed;NoComparison>] Factory() = 
        member __.Resolve (NotNull "descriptor" descriptor : IViewDescriptor) : IView = 
            let flags = BindingFlags.Public|||BindingFlags.Instance
            let constructors = 
                descriptor.ViewType.GetConstructors(flags) 
                |> Array.toList
                |> List.filter (fun c -> c.GetParameters().Length = 0) 
            match constructors with
            | [] -> raise (InvalidOperationException(String.Format("View `{0}` does not have suitable constructor", descriptor.ViewType.FullName)))
            | head::_ -> downcast head.Invoke([||]) : IView

        member __.Resolve (NotNull "descriptor" descriptor : IViewDescriptor, NotNull "model" model : obj) : IView = 
            let flags = BindingFlags.Public|||BindingFlags.Instance
            let constructors = 
                descriptor.ViewType.GetConstructors(flags) 
                |> Array.toList
                |> List.filter (fun c -> c.GetParameters().Length = 1 && c.GetParameters().[0].ParameterType.GetTypeInfo().IsAssignableFrom(model.GetType().GetTypeInfo())) 
            match constructors with
            | [] -> raise (InvalidOperationException(String.Format("View `{0}` does not have suitable constructor", descriptor.ViewType.FullName)))
            | head::_ -> downcast head.Invoke([|model|]) : IView
        
        interface IViewFactory with 
            member this.Resolve d = this.Resolve d
            member this.Resolve (d, m) = this.Resolve(d, m)
