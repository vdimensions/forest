namespace Forest

open System
open System.Collections.Generic
open System.Collections.Immutable
open Axle
open Forest
open Forest.ComponentModel
open Forest.Engine
open Forest.Engine.Instructions
open Forest.StateManagement
open Forest.UI
  
module internal ForestExecutionContext =

    let getDescriptor (ctx : IForestContext) (handle : ViewHandle) =
        match ctx.ViewRegistry.GetDescriptor handle |> null2vopt with
        | ValueSome d -> Ok d
        | ValueNone -> Runtime.Error.NoDescriptor handle |> Error

    let createRuntimeView 
            (executionContext : IForestExecutionContext) (ctx : IForestContext) (views : IDictionary<thash, IRuntimeView>)
            (node : Tree.Node) (model : obj option) (descriptor : IViewDescriptor) =
        try
            let view = 
                match model with 
                | Some m -> ctx.ViewFactory.Resolve(descriptor, m)
                | None -> ctx.ViewFactory.Resolve(descriptor)
                :?> IRuntimeView
            views.[node.InstanceID] <- view
            view.AcquireContext (node, descriptor, executionContext)
            view |> Ok
        with 
        | e -> View.Error.InstantiationError(InstantiateViewInstruction(node, (model |> opt2ns).Value),  e) |> Runtime.Error.ViewError |> Error

type [<Sealed;NoComparison>] internal ForestExecutionContext private (t : Tree, pv : ImmutableDictionary<thash, IPhysicalView>, ctx : IForestContext, sp : IForestStateProvider, dp : PhysicalViewDomProcessor, eventBus : IEventBus, viewStates : ImmutableDictionary<thash, ViewState>, logicalViews : ImmutableDictionary<thash, IRuntimeView>, changeLog : System.Collections.Generic.List<ViewStateChange>) =
    inherit Forest.Engine.SlaveExecutionContext(ctx, dp, eventBus, t, viewStates, logicalViews, pv)

    new (t : Tree, viewState : ImmutableDictionary<thash, ViewState>, views : ImmutableDictionary<thash, IRuntimeView>, pv : ImmutableDictionary<thash, IPhysicalView>, ctx : IForestContext, sp : IForestStateProvider, dp : PhysicalViewDomProcessor)
        = new ForestExecutionContext(t, pv, ctx, sp, dp, new EventBus(), viewState, views, System.Collections.Generic.List())

    [<DefaultValue;ThreadStatic>]
    static val mutable private _currentEngine : IForestExecutionContext voption

    //let prepareView (changeLog : System.Collections.Generic.List<ViewStateChange>) (t : Tree) (node : Tree.Node) (model : obj option) (view : IRuntimeView)  =
    //    self.UpdateTree t
    //    match model with
    //    | Some m -> ViewStateChange.ViewAddedWithModel(node, ViewState.Create(m))
    //    | None -> ViewStateChange.ViewAdded(node, ViewState.Empty)
    //    |> changeLog.Add
    //    view.Load()
    //    (upcast view : IView)
    
    //let instantiateView (executionContext : IForestExecutionContext) (ctx : IForestContext) (node : Tree.Node) (model : obj option) (d : IViewDescriptor) =
    //    ForestExecutionContext.createRuntimeView executionContext ctx views node model d
    //    |> Result.map (prepareView changeLog (tree.Insert node) node model)
    //    |> Result.map Runtime.Status.ViewCreated

    member private this.Init() =
        for node in this._tree.Roots do
            if (not <| Tree.Node.Shell.Equals node) && (not <| logicalViews.ContainsKey node.InstanceID) then 
                let handle = node.ViewHandle
                match handle |> ForestExecutionContext.getDescriptor ctx |> Result.bind (ForestExecutionContext.createRuntimeView this ctx logicalViews node None) with
                | Ok view ->
                    view.Resume(viewStates.[node.InstanceID])
                | _ -> ignore()
        this

    member internal this.base_ProcessInstructions i = this.ProcessInstructions i
    

    member this.Apply (entry : ViewStateChange) =
        match entry with
        | ViewStateChange.ViewAddedWithModel (node, viewState) ->
            match Tree.Node.Shell.Equals node with
            | true -> Some (Runtime.Error.SubTreeAbsent node)
            | false ->
                let hs = this._tree.Insert node
                match viewStates.TryGetValue node.InstanceID with
                | (true, _) -> Some (Runtime.Error.SubTreeNotExpected node)
                | (false, _) ->
                    node.ViewHandle
                    |> ForestExecutionContext.getDescriptor ctx 
                    |> Result.bind (ForestExecutionContext.createRuntimeView this ctx logicalViews node (Some viewState.Model))
                    |> Result.map (
                        fun view ->
                            this.UpdateTree hs
                            view.Resume(viewState)
                        )
                    |> Result.error
        | ViewStateChange.ViewAdded (node, viewState) ->
            match Tree.Node.Shell.Equals node with
            | true -> Some (Runtime.Error.SubTreeAbsent node)
            | false ->
                let hs = this._tree.Insert node
                match viewStates.TryGetValue node.InstanceID with
                | (true, _) -> Some (Runtime.Error.SubTreeNotExpected node)
                | (false, _) ->
                    node.ViewHandle
                    |> ForestExecutionContext.getDescriptor ctx 
                    |> Result.bind (ForestExecutionContext.createRuntimeView this ctx logicalViews node (Some viewState.Model))
                    |> Result.map (
                        fun view ->
                            this.UpdateTree hs
                            view.Resume(viewState)
                        )
                    |> Result.error
        // TODO: 
        //| ViewStateChange.ViewDestroyed (node) -> 
        //    destroyView node |> Result.error
        | ViewStateChange.ViewStateUpdated (node, state) -> 
            logicalViews.[node.InstanceID].Resume state
            None

    member this.UpdateTree t = this._tree <- t

    static member Current with get() = ForestExecutionContext._currentEngine

    static member Create (ctx : IForestContext, sp : IForestStateProvider, dp : PhysicalViewDomProcessor) = 
        let state = sp.LoadState()
        let ctx = new ForestExecutionContext(state.Tree, state.ViewStates, state.LogicalViews, state.PhysicalViews, ctx, sp, dp)
        ForestExecutionContext._currentEngine <- ValueSome (ctx :> IForestExecutionContext)
        ctx.Init()

    member this.base_Dispose() = base.Dispose();
    override this.Dispose() = 
        ForestExecutionContext._currentEngine <- ValueNone
        this.base_Dispose();
