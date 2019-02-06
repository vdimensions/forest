namespace Forest

open System
open System.Diagnostics
open Axle.Option
open Axle.Verification
open Forest


type [<AbstractClass;NoComparison>] LogicalView<[<EqualityConditionalOn>]'T>(vm : 'T) =
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private hierarchyKey : TreeNode
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private runtime : IForestRuntime
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private descriptor : IViewDescriptor
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable vm : 'T = vm

    override this.Finalize() =
        this.Dispose(false)

    member this.Publish<'M> (message : 'M, [<ParamArray>] topics : string[]) = 
        this.runtime.PublishEvent this message topics

    abstract member Load : unit -> unit
    default __.Load() = ()

    abstract member Resume : unit -> unit
    default __.Resume() = ()

    abstract member Dispose : bool -> unit
    default __.Dispose(_) = ()

    member this.FindRegion (NotNull "name" name) = 
        upcast RegionImpl(name, this) : IRegion

    member this.Close() =
        match null2vopt this.runtime with
        | ValueNone ->
            invalidOp("No runtime available!")
        | ValueSome rt -> 
            let (parent, region) = (this.hierarchyKey.Parent, this.hierarchyKey.Region)
            rt.RemoveViewFromRegion parent region (System.Predicate(fun v -> obj.ReferenceEquals(v, this)))

    member this.UpdateModel (NotNull "updateFn" updateFn : Func<'T, 'T>) : unit =
        let newModel = updateFn.Invoke(vm)
        vm <-
            match null2vopt this.runtime with
            // The default behaviour
            | ValueSome rt -> (rt.SetViewModel false this.hierarchyKey newModel)
            // This case is entered if the view model is set at construction time, for example, by a DI container.
            | ValueNone -> newModel


    member __.Model with get ():'T = vm

    member internal this.HierarchyKey with get() = this.hierarchyKey

    interface IRuntimeView with
        member this.Load () = this.Load()

        member this.Resume model =
            vm <- this.runtime.SetViewModel true this.hierarchyKey (downcast model : 'T)
            this.Resume()

        member this.AcquireRuntime (node:TreeNode) (vd:IViewDescriptor) (NotNull "runtime" runtime:IForestRuntime) =
            match null2vopt this.runtime with
            | ValueNone ->
                this.descriptor <- vd
                this.hierarchyKey <- node
                match runtime.GetViewModel this.HierarchyKey with
                | Some viewModelFromState -> vm <- (downcast viewModelFromState : 'T)
                | None -> ignore <| runtime.SetViewModel true this.HierarchyKey vm
                runtime.SubscribeEvents this
                this.runtime <- runtime
                ()
            | ValueSome _ -> invalidOp(String.Format("View {0} is already captured by a runtime", this.hierarchyKey.View))

        member this.AbandonRuntime (_) =
            match null2vopt this.runtime with
            | ValueSome currentModifier ->
                currentModifier.UnsubscribeEvents this
                match currentModifier.GetViewModel this.HierarchyKey with
                | Some viewModelFromState -> 
                    vm <- (downcast viewModelFromState : 'T)
                    ()
                | None -> () 
                this.runtime <- Unchecked.defaultof<IForestRuntime>
            | ValueNone -> ()

        member this.InstanceID with get() = this.hierarchyKey
        member this.Descriptor with get() = this.descriptor
        member this.Runtime with get() = this.runtime

    interface IView<'T> with
        member this.UpdateModel fn = this.UpdateModel fn
        member this.Model with get() = this.Model

    interface IView with
        member this.Publish (m, t) = this.Publish (m, t)
        member this.FindRegion name = this.FindRegion name
        member this.Close() = this.Close()
        member this.Model with get() = upcast this.Model

    interface IDisposable with
        member this.Dispose() =
            this.Dispose(true)
            this |> GC.SuppressFinalize

 and private RegionImpl(regionName : rname, owner : IRuntimeView) =
    member __.ActivateView (NotNull "viewName" viewName : vname) =
        owner.Runtime.ActivateView((ViewHandle.ByName viewName), regionName, owner.InstanceID)

    member __.ActivateView (NotNull "viewName" viewName : vname, NotNull "model" model : 'm) =
        owner.Runtime.ActivateView((ViewHandle.ByName viewName), regionName, owner.InstanceID, model)

    member __.ActivateView<'v when 'v :> IView> () : 'v =
        owner.Runtime.ActivateView((ViewHandle.ByType typeof<'v>), regionName, owner.InstanceID) :?> 'v

    member __.ActivateView<'v, 'm when 'v :> IView<'m>> (model : 'm) : 'v =
        owner.Runtime.ActivateView((ViewHandle.ByType typeof<'v>), regionName, owner.InstanceID, model) :?> 'v

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

type [<AbstractClass>] LogicalView() = inherit LogicalView<Unit>(())