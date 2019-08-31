namespace Forest

open System
open System.Collections.Generic
open Axle
open Axle.Verification
open Forest
open Forest.ComponentModel
open Forest.Engine
open Forest.Engine.Instructions
open Forest.Templates
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

type [<Sealed;NoComparison>] internal ForestExecutionContext private (t : Tree, pv : Map<thash, IPhysicalView>, ctx : IForestContext, sp : IForestStateProvider, dp : PhysicalViewDomProcessor, eventBus : IEventBus, viewStates : System.Collections.Generic.Dictionary<thash, ViewState>, views : System.Collections.Generic.Dictionary<thash, IRuntimeView>, changeLog : System.Collections.Generic.List<ViewStateChange>) as self =
    inherit Forest.Engine.ForestExecutionContext(t, ctx, eventBus, views, viewStates)

    new (t : Tree, viewState : Map<thash, ViewState>, views : Map<thash, IRuntimeView>, pv : Map<thash, IPhysicalView>, ctx : IForestContext, sp : IForestStateProvider, dp : PhysicalViewDomProcessor)
        = new ForestExecutionContext(t, pv, ctx, sp, dp, new EventBus(), System.Collections.Generic.Dictionary(viewState, StringComparer.Ordinal), System.Collections.Generic.Dictionary(views, StringComparer.Ordinal), System.Collections.Generic.List())

    [<DefaultValue;ThreadStatic>]
    static val mutable private _currentEngine : IForestExecutionContext voption

    //[<DefaultValue>]
    //val mutable private _nestedCalls : int

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

    let getRegionContents (owner : Tree.Node) (region : rname) =
        let isInRegion (node:Tree.Node) = StringComparer.Ordinal.Equals(region, node.Region)
        (self.GetTree()).Filter(new Predicate<Tree.Node>(isInRegion), owner)

    //let clearRegion (owner : Tree.Node) (region : rname) =
    //    let errors = [
    //        for child in getRegionContents owner region do
    //            match destroyView child with
    //            | Error e -> yield e
    //            | _ -> ()
    //    ]
    //    match errors with 
    //    | [] -> Runtime.Status.RegionCleared |> Ok
    //    | errLsit -> errLsit |> Runtime.Error.Multiple |> Error

    //let removeViewsFromRegion (owner : Tree.Node) (region : rname) (predicate : System.Predicate<IView>)=
    //    let mutable errors = List.empty<Runtime.Error>
    //    let mutable successes = List.empty<Runtime.Status>
    //    for child in getRegionContents owner region do
    //        if (predicate.Invoke(views.[child.InstanceID])) then
    //            match destroyView child with
    //            | Ok x -> successes <- x :: successes
    //            | Error e -> errors <-  e :: errors
    //    match errors with
    //    | [] -> successes |> Runtime.Status.Multiple |> Ok
    //    | list -> list |> Runtime.Error.Multiple |> Error      

    //let rec processChanges (processInstructionsCall : ForestInstruction[] -> unit) (executionContext: ForestExecutionContext) (ctx : IForestContext) (operations : ForestInstruction list) =
    //    ////executionContext._nestedCalls <- (executionContext._nestedCalls + 1)
    //    ////try
    //    //    match operations with
    //    //    | [operation] ->
    //    //        match operation with
    //    //        | :? UpdateModelInstruction as um -> 
    //    //            operation |> Array.singleton |> processInstructionsCall
    //    //            //match viewStates.TryGetValue um.Node.InstanceID with
    //    //            //| (true, vs) -> viewStates.[um.Node.InstanceID] <- ViewState.UpdateModel(vs, um.Model)
    //    //            //| (false, _) -> viewStates.[um.Node.InstanceID] <- ViewState.Create(um.Model)
    //    //            Runtime.Status.ModelUpdated um.Model |> Ok
    //    //        | :? DestroyViewInstruction as dv -> 
    //    //            operation |> Array.singleton |> processInstructionsCall
    //    //            //let tt = self.GetTree()
    //    //            //let (t, nodes) = tt.Remove(node)
    //    //            //for removedNode in nodes do
    //    //            //    let removedHash = removedNode.InstanceID
    //    //            //    let view = views.[removedHash]
    //    //            //    view.Dispose()
    //    //            //    viewStates.Remove removedHash |> ignore
    //    //            //    views.Remove removedHash |> ignore
    //    //            //    removedNode |> ViewStateChange.ViewDestroyed |> changeLog.Add
    //    //            //self.UpdateTree t
    //    //            Runtime.Status.ViewDestoyed |> Ok
    //    //        | :? InvokeCommandInstruction as ic -> 
    //    //            //match views.TryGetValue ic.InstanceID with
    //    //            //| (true, view) ->
    //    //            //    match view.Descriptor.Commands.TryGetValue ic.CommandName with
    //    //            //    | (true, cmd) -> 
    //    //            //        try
    //    //            //            cmd.Invoke (view, ic.CommandArg)
    //    //            //            Runtime.Status.CommandInvoked |> Ok
    //    //            //        with
    //    //            //        | cause -> Command.InvocationError(view.Descriptor.ViewType, ic.CommandName, cause) |> Runtime.Error.CommandError |> Error
    //    //            //    | (false, _) ->  Command.Error.CommandNotFound(view.Descriptor.ViewType, ic.CommandName) |> Runtime.Error.CommandError |> Error
    //    //            //| (false, _) -> Runtime.Error.ViewstateAbsent ic.InstanceID |> Error
    //    //            operation |> Array.singleton |> processInstructionsCall
    //    //            Runtime.Status.CommandInvoked |> Ok
    //    //        | :? SendMessageInstruction as sm -> 
    //    //            operation |> Array.singleton |> processInstructionsCall
    //    //            //let (senderID, message, topics) = null2opt(sm.SenderInstanceID), sm.Message, sm.Topics
    //    //            //match senderID with
    //    //            //| Some id ->
    //    //            //    match views.TryGetValue id with
    //    //            //    | (true, sender) -> 
    //    //            //        eventBus.Publish(sender, message, topics)
    //    //            //        Runtime.Status.MesssagePublished |> Ok
    //    //            //    // TODO: should be error
    //    //            //    | _ -> Runtime.Status.MessageSourceNotFound |> Ok
    //    //            //| None ->
    //    //            //    eventBus.Publish(Unchecked.defaultof<IView>, message, topics)
    //    //            //    Runtime.Status.MesssagePublished |> Ok
    //    //            Runtime.Status.MesssagePublished |> Ok
    //    //        | :? ClearRegionInstruction as cr ->
    //    //            operation |> Array.singleton |> processInstructionsCall
    //    //            //clearRegion cr.Node cr.Region
    //    //            Runtime.Status.RegionCleared |> Ok
    //    //        | :? InstantiateViewInstruction as iv -> 
    //    //            operation |> Array.singleton |> processInstructionsCall
    //    //            //    iv.Node.ViewHandle
    //    //            //    |> ForestExecutionContext.getDescriptor ctx
    //    //            //    |> Result.bind (
    //    //            //        fun descriptor -> 
    //    //            //            instantiateView executionContext ctx iv.Node (null2opt iv.Model) descriptor
    //    //            //        )
    //    //            (upcast views.[iv.Node.InstanceID] : IView) |> Runtime.Status.ViewCreated |> Result.Ok
    //    //    | operations -> 
    //    //        let mappedResults =
    //    //            iterateStates processInstructionsCall executionContext ctx operations
    //    //            |> List.map (fun x -> x |> Result.ok, x |> Result.error )
    //    //        let errors = mappedResults |> List.map snd |> List.choose id
    //    //        if errors |> List.isEmpty 
    //    //        then 
    //    //            mappedResults |> List.map fst |> List.choose id |> Runtime.Status.Multiple |> Ok
    //    //        else errors |> Runtime.Error.Multiple |> Error
    //    //    //| _ -> Error (UnknownOperation operation)
    //    ////finally
    //    ////    executionContext._nestedCalls <- (executionContext._nestedCalls - 1)
    //    operations
    //    |> Array.ofList
    //    |> processInstructionsCall
    //and iterateStates processInstructionsCall executionContext ctx ops =
    //    match ops with
    //    | [] -> []
    //    | [op] -> [ processChanges processInstructionsCall executionContext ctx [op] ]
    //    | head::tail -> processChanges processInstructionsCall executionContext ctx [head] :: iterateStates processInstructionsCall executionContext ctx tail

    member private this.Init() =
        for kvp in views do 
            let (view, n, d) = (kvp.Value, kvp.Value.Node, kvp.Value.Descriptor)
            view.AcquireContext (n, d, this)
        for node in this._tree.Roots do
            if (not <| Tree.Node.Shell.Equals node) && (not <| views.ContainsKey node.InstanceID) then 
                let handle = node.ViewHandle
                match handle |> ForestExecutionContext.getDescriptor ctx |> Result.bind (ForestExecutionContext.createRuntimeView this ctx views node None) with
                | Ok view ->
                    view.Resume(viewStates.[node.InstanceID])
                | _ -> ignore()
        this

    member internal this.ActivateView (node : Tree.Node) =
        (InstantiateViewInstruction(node, null))
        |> this.ActivateView 
        //|> Runtime.resolve (function
        //    | Runtime.Status.ViewCreated view -> view
        //    | _ -> Unchecked.defaultof<_>
        //)
    member internal this.ActivateView (node : Tree.Node, model : obj) =
        (InstantiateViewInstruction(node, model))
        |> this.ActivateView 
        //|> Runtime.resolve (function
        //    | Runtime.Status.ViewCreated view -> view
        //    | _ -> Unchecked.defaultof<_>
        //)

    member internal this.GetOrActivateView (node : Tree.Node) : 'TView when 'TView :> IView =
        let result =
            match node.InstanceID |> views.TryGetValue with
            | (true, viewState) -> (upcast viewState : IView)
            | (false, _) -> this.ActivateView node
        downcast result:'TView
    member internal this.base_ProcessInstructions i = this.ProcessInstructions i
    //member this.Do (operation : ForestInstruction list) =
    //    match processChanges (this.base_ProcessInstructions) this ctx operation with
    //    | Ok something ->
    //        if (this._nestedCalls = 0) then
    //            this._eventBus.ProcessMessages() |> ignore
    //        Ok something
    //    | Error e -> Error e

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
                    |> Result.bind (ForestExecutionContext.createRuntimeView this ctx views node (Some viewState.Model))
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
                    |> Result.bind (ForestExecutionContext.createRuntimeView this ctx views node (Some viewState.Model))
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
            views.[node.InstanceID].Resume state
            None

    member this.GetTree () : Tree = this._tree
    member this.UpdateTree t = this._tree <- t

    static member Current with get() = ForestExecutionContext._currentEngine

    static member Create (ctx : IForestContext, sp : IForestStateProvider, dp : PhysicalViewDomProcessor) = 
        let state = sp.LoadState()
        let ctx = new ForestExecutionContext(state.Tree, state.ViewState, state.Views, state.PhysicalViews, ctx, sp, dp)
        ForestExecutionContext._currentEngine <- ValueSome (ctx :> IForestExecutionContext)
        ctx.Init()

    override this.Dispose() = 
        ForestExecutionContext._currentEngine <- ValueNone
        try
            for kvp in views do 
                kvp.Value.AbandonContext this
            this._eventBus.Dispose()
            let a, b, c, cl = this.Deconstruct()
            dp.PhysicalViews <- pv
            State.create(a, b, c, pv) |> State.traverse (ForestDomRenderer(seq { yield dp :> IDomProcessor }, ctx))
            let newPv = dp.PhysicalViews
            let newState = State.create(a, b, c, newPv)
                //match fuid with
                //| Some f -> State.createWithFuid(a, b, c, f)
                //| None -> State.create(a, b, c)
            sp.CommitState newState
        with
        | e -> 
            sp.RollbackState()
            raise e

    member internal this.Deconstruct() = 
        (
            this._tree, 
            viewStates |> Seq.map (|KeyValue|) |> Map.ofSeq, 
            views |> Seq.map (|KeyValue|) |> Map.ofSeq, 
            changeLog |> List.ofSeq
        )

    override this.RegisterSystemView<'sv when 'sv :> ISystemView> () = 
        let descriptor = 
            match typeof<'sv> |> ctx.ViewRegistry.GetDescriptor |> null2vopt with
            | ValueNone -> 
                ctx.ViewRegistry
                |> ViewRegistry.registerViewType typeof<'sv> 
                |> ViewRegistry.getDescriptorByType typeof<'sv> 
            | ValueSome d -> d
        let key = Tree.Node.Create(Tree.Node.Shell.Region, descriptor.Name, Tree.Node.Shell)
        this.GetOrActivateView<'sv> key

    override this.Navigate (NotNullOrEmpty "name" name) =
        if (this._nestedCalls > 1) then
            this._eventBus.ClearDeadLetters() |> ignore
        Template.LoadTemplate(ctx.TemplateProvider, name)
        |> Templates.TemplateCompiler.compileOps
        |> Array.ofList
        |> this.base_ProcessInstructions

    override this.Navigate (NotNullOrEmpty "name" name, message) =
        if (this._nestedCalls > 1) then
            this._eventBus.ClearDeadLetters() |> ignore
        [
            yield (upcast SendMessageInstruction(message, [||], null) : ForestInstruction)
            yield! Template.LoadTemplate(ctx.TemplateProvider, name) |> Templates.TemplateCompiler.compileOps
        ]
        |> Array.ofList
        |> this.base_ProcessInstructions   

    // ------------------

    override __.GetViewState node = 
        match viewStates.TryGetValue node.InstanceID with
        | (true, vs) -> vs |> Nullable
        | (false, _) -> Nullable()

    override __.SetViewState(silent, node, vs) = 
        viewStates.[node.InstanceID] <- vs
        if not silent then changeLog.Add(ViewStateChange.ViewStateUpdated(node, vs))
        vs

    override __.GetRegionContents (node, region) =
        getRegionContents node region 
        |> Seq.map (fun node -> (upcast views.[node.InstanceID] : IView))

    //override __.RemoveViewFromRegion (node, region, predicate) =
    //    removeViewsFromRegion node region predicate 
    //    |> Runtime.resolve ignore
