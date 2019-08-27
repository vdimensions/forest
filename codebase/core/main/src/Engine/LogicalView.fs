namespace Forest

open System
open System.Diagnostics
open Axle.Option
open Axle.Verification
open Forest
open Forest.ComponentModel


type [<AbstractClass;NoComparison>] LogicalView<[<EqualityConditionalOn>] 'T> private(state : ViewState) =
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private hierarchyKey : Tree.Node
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private executionContext : IForestExecutionContext
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    [<DefaultValue>]
    val mutable private descriptor : IViewDescriptor
    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable state : ViewState = state

    let updateViewState fn (v : LogicalView<'T>) =
        state <-
            match null2vopt v.executionContext with
            // The default behaviour
            | ValueSome context -> 
                match context.GetViewState v.hierarchyKey with
                | Some vs -> fn vs
                | None -> invalidOp "ViewState not found"
                |> context.SetViewState false v.hierarchyKey
            | ValueNone -> invalidOp "This operation cannot be applied without execution context"

    new (model : 'T) = new LogicalView<'T>(model |> ViewState.withModelUnchecked)

    override this.Finalize() =
        this.Dispose(false)

    member this.Publish<'M> (message : 'M, [<ParamArray>] topics : string[]) = 
        this.executionContext.PublishEvent this message topics

    abstract member Load : unit -> unit
    default __.Load() = ()

    abstract member Resume : unit -> unit
    default __.Resume() = ()

    member this.DisableCommand(NotNull "command" command : cname) =
        this |> updateViewState (fun state -> ViewState.DisableCommand(state, command))

    member this.EnableCommand(NotNull "command" command : cname) =
        this |> updateViewState (fun state -> ViewState.EnableCommand(state, command))

    member this.DisableLink(NotNull "link" link : cname) =
        this |> updateViewState (fun state -> ViewState.DisableLink(state, link))

    member this.EnableLink(NotNull "link" link : string) =
        this |> updateViewState (fun state -> ViewState.EnableLink(state, link))
        
    abstract member Dispose : bool -> unit
    default __.Dispose(_) = ()

    member this.FindRegion (NotNull "name" name) = 
        upcast RegionImpl(name, this) : IRegion

    member this.Close() =
        match null2vopt this.executionContext with
        | ValueNone ->
            invalidOp("No runtime available!")
        | ValueSome rt -> 
            let (parent, region) = (this.hierarchyKey.Parent, this.hierarchyKey.Region)
            rt.RemoveViewFromRegion parent region (System.Predicate(fun v -> obj.ReferenceEquals(v, this)))

    member this.UpdateModel (NotNull "updateFn" updateFn : Func<'T, 'T>) : unit =
        let newModel = updateFn.Invoke(this.Model)
        state <-
            match null2vopt this.executionContext with
            // The default behaviour
            | ValueSome rt -> 
                match rt.GetViewState this.hierarchyKey with
                | Some vs -> ViewState.UpdateModel(vs, newModel)
                | None -> ViewState.Create(newModel)
                |> rt.SetViewState false this.hierarchyKey
            // This case is entered if the view model is set at construction time, for example, by a DI container.
            | ValueNone -> newModel |> ViewState.withModel

    member this.Engine with get() = this.executionContext :> IForestEngine

    member __.Model with get ():'T = state.Model :?> 'T

    member internal this.HierarchyKey with get() = this.hierarchyKey

    interface IRuntimeView with
        member this.Load () = 
            this.Load()

        member this.Resume viewState =
            state <- this.executionContext.SetViewState true this.hierarchyKey viewState
            this.Resume()

        member this.AcquireContext (node : Tree.Node) (vd : IViewDescriptor) (NotNull "context" context : IForestExecutionContext) =
            match null2vopt this.executionContext with
            | ValueNone ->
                this.descriptor <- vd
                this.hierarchyKey <- node
                match context.GetViewState this.HierarchyKey with
                | Some viewState -> state <- viewState
                | None -> ignore <| context.SetViewState true this.HierarchyKey state
                this.executionContext <- context
                this.executionContext.SubscribeEvents this
                ()
            | ValueSome _ -> invalidOp(String.Format("View {0} is already captured by a context", this.hierarchyKey.View))

        member this.AbandonContext (_) =
            match null2vopt this.executionContext with
            | ValueSome context ->
                context.UnsubscribeEvents this                
                // FIXME: when exception occurs, context is not abandoned
                match context.GetViewState this.HierarchyKey with 
                | Some viewState -> state <- viewState
                | None -> () 
                this.executionContext <- Unchecked.defaultof<IForestExecutionContext>
            | ValueNone -> ()

        member this.InstanceID with get() = this.hierarchyKey
        member this.Descriptor with get() = this.descriptor
        member this.Context with get() = this.executionContext

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
            try this.Dispose(true)
            // When disposing, always abandon the runtime
            finally (this :> IRuntimeView).AbandonContext(this.executionContext)
            this |> GC.SuppressFinalize

 and [<Sealed;NoComparison>] private RegionImpl(regionName : rname, owner : IRuntimeView) =
    member __.ActivateView (NotNull "viewName" viewName : vname) =
        let result = owner.Context.ActivateView((ViewHandle.ByName viewName), regionName, owner.InstanceID)
        //owner.Context.ProcessMessages()
        result

    member __.ActivateView (NotNull "viewName" viewName : vname, NotNull "model" model : obj) =
        let result = owner.Context.ActivateView((ViewHandle.ByName viewName), regionName, owner.InstanceID, model)
        //owner.Context.ProcessMessages()
        result

    member __.ActivateView (NotNull "viewType" viewType : Type) : IView =
        let result = owner.Context.ActivateView((ViewHandle.ByType viewType), regionName, owner.InstanceID)
        //owner.Context.ProcessMessages()
        result

    member __.ActivateView (NotNull "viewType" viewType : Type, NotNull "model" model : obj) : IView =
        let result = owner.Context.ActivateView((ViewHandle.ByType viewType), regionName, owner.InstanceID, model)
        //owner.Context.ProcessMessages()
        result

    member __.ActivateView<'v when 'v :> IView> () : 'v =
        let result = owner.Context.ActivateView((ViewHandle.ByType typeof<'v>), regionName, owner.InstanceID) :?> 'v
        //owner.Context.ProcessMessages()
        result

    member __.ActivateView<'v, 'm when 'v :> IView<'m>> (NotNull "model" model : 'm) : 'v =
        let result = owner.Context.ActivateView((ViewHandle.ByType typeof<'v>), regionName, owner.InstanceID, model) :?> 'v
        //owner.Context.ProcessMessages()
        result

    member this.Clear() =
        owner.Context.ClearRegion owner.InstanceID regionName
        this

    member this.Remove(NotNull "predicate" predicate : System.Predicate<IView>) =
        owner.Context.RemoveViewFromRegion owner.InstanceID regionName predicate
        this

    member __.GetContents() =
        owner.Context.GetRegionContents owner.InstanceID regionName

    member __.Name 
        with get() = regionName

    interface IRegion with
        member this.ActivateView (viewName : vname) = this.ActivateView viewName
        member this.ActivateView (viewName : vname, model : obj) = this.ActivateView(viewName, model)
        member this.ActivateView (viewType : Type) = this.ActivateView(viewType)
        member this.ActivateView (viewType : Type, model : obj) = this.ActivateView(viewType, model)
        member this.ActivateView<'v when 'v :> IView>() = this.ActivateView<'v>()
        member this.ActivateView<'v, 'm when 'v :> IView<'m>> (model : 'm) = this.ActivateView<'v, 'm>(model)
        member this.Clear() = upcast this.Clear()
        member this.Remove predicate = upcast this.Remove(predicate)
        member this.Name = this.Name
        member this.Views = this.GetContents()

type [<AbstractClass;NoComparison>] LogicalView() = inherit LogicalView<Unit>(())