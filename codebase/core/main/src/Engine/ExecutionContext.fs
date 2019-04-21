namespace Forest

open System
open System.Collections.Generic
open Axle
open Axle.Option
open Axle.Verification
open Forest
open Forest.Collections
open Forest.Events
open Forest.Templates
open Forest.Templates.Raw
open Forest.UI


type [<Sealed;NoComparison>] internal ForestExecutionContext private (t : Tree, models : Map<thash, obj>, views : Map<thash, IRuntimeView>, pv : Map<thash, IPhysicalView>, ctx : IForestContext, sp : IForestStateProvider, renderer : IPhysicalViewRenderer) as self = 
    let mutable tree = t
    let eventBus : IEventBus = Event.createEventBus()
    let models : System.Collections.Generic.Dictionary<thash, obj> = System.Collections.Generic.Dictionary(models, StringComparer.Ordinal)
    let views : System.Collections.Generic.Dictionary<thash, IRuntimeView> = System.Collections.Generic.Dictionary(views, StringComparer.Ordinal)
    let changeLog : System.Collections.Generic.List<StateChange> = System.Collections.Generic.List()

    [<ThreadStatic>]
    [<DefaultValue>]
    static val mutable private _currentEngine : IForestExecutionContext voption

    let getDescriptor (handle : ViewHandle) =
        match ctx.ViewRegistry |> ViewRegistry.getDescriptor handle |> null2vopt with
        | ValueSome d -> Ok d
        | ValueNone -> Runtime.Error.NoDescriptor handle |> Error

    let updateModel (id : thash) (m : obj) : Result<Runtime.Status, Runtime.Error> =
        models.[id] <- m
        Runtime.Status.ModelUpdated m |> Ok

    let destroyView (node : TreeNode) : Result<Runtime.Status, Runtime.Error> =
        let (t, nodes) = tree |> Tree.remove node
        for removedNode in nodes do
            let removedHash = removedNode.Hash
            let view = views.[removedHash]
            view.Dispose()
            models.Remove removedHash |> ignore
            views.Remove removedHash |> ignore
            removedNode |> StateChange.ViewDestroyed |> changeLog.Add
        tree <- t
        Runtime.Status.ViewDestoyed |> Ok

    let getRegionContents (owner : TreeNode) (region : rname) =
        let isInRegion (node:TreeNode) = StringComparer.Ordinal.Equals(region, node.Region)
        tree |> Tree.filter owner isInRegion

    let clearRegion (owner : TreeNode) (region : rname) =
        let mutable errors = List.empty<Runtime.Error>
        for child in getRegionContents owner region do
            match destroyView child with
            | Error e -> errors <-  e::errors
            | _ -> ()
        match errors with 
        | [] -> Runtime.Status.RegionCleared |> Ok
        | errLsit -> errLsit |> Runtime.Error.Multiple |> Error

    let removeViewsFromRegion (owner : TreeNode) (region : rname) (predicate : System.Predicate<IView>)=
        let mutable errors = List.empty<Runtime.Error>
        let mutable successes = List.empty<Runtime.Status>
        for child in getRegionContents owner region do
            if (predicate.Invoke(views.[child.Hash])) then
                match destroyView child with
                | Ok x -> successes <- x :: successes
                | Error e -> errors <-  e :: errors
        match errors with
        | [] -> successes |> Runtime.Status.Multiple |> Ok
        | list -> list |> Runtime.Error.Multiple |> Error

    let executeCommand (name : cname) (stateKey : thash) (arg : obj) =
        match views.TryGetValue stateKey with
        | (true, view) ->
            match view.Descriptor.Commands.TryFind name with
            | Some cmd -> 
                try
                    view |> cmd.Invoke arg
                    Runtime.Status.CommandInvoked |> Ok
                with
                | cause -> Command.InvocationError(view.Descriptor.ViewType, name, cause) |> Runtime.Error.CommandError |> Error
            | None ->  Command.Error.CommandNotFound(view.Descriptor.ViewType, name) |> Runtime.Error.CommandError |> Error
        | (false, _) -> Runtime.Error.ViewstateAbsent stateKey |> Error

    let publishEvent (senderID : thash option) (message : 'm) (topics : string array) =
        match senderID with
        | Some id ->
            match views.TryGetValue id with
            | (true, sender) -> 
                eventBus.Publish(sender, message, topics)
                Runtime.Status.MesssagePublished |> Ok
            // TODO: should be error
            | _ -> Runtime.Status.MessageSourceNotFound |> Ok
        | None ->
            eventBus.Publish(Unchecked.defaultof<IView>, message, topics)
            Runtime.Status.MesssagePublished |> Ok

    let rec processChanges (ctx : IForestContext) (operation : Runtime.Operation) =
        match operation with
        | Runtime.Operation.Multiple operations -> 
            let mappedResults =
                iterateStates ctx operations
                |> List.map (fun x -> x |> Result.ok, x |> Result.error )
            let errors = mappedResults |> List.map snd |> List.choose id
            if errors |> List.isEmpty 
            then mappedResults |> List.map fst |> List.choose id |> Runtime.Status.Multiple |> Ok
            else errors |> Runtime.Error.Multiple |> Error
        | Runtime.Operation.InstantiateView (viewHandle, region, parent, model) ->
            viewHandle
            |> getDescriptor
            |> Result.bind (
                fun descriptor -> 
                    let node = TreeNode.newKey region (descriptor.Name) parent 
                    self.instantiateView viewHandle node model descriptor
                )
        | Runtime.Operation.InstantiateViewByNode (node, model) ->
            let vhFromNode = node |> ViewHandle.fromNode
            vhFromNode
            |> getDescriptor
            |> Result.bind (
                fun descriptor -> 
                    self.instantiateView vhFromNode node model descriptor
                )
        | Runtime.Operation.UpdateModel (viewID, model) -> 
            updateModel viewID model
        | Runtime.Operation.DestroyView viewID -> 
            destroyView viewID
        | Runtime.Operation.InvokeCommand (commandName, viewID, arg) -> 
            executeCommand commandName viewID arg
        | Runtime.Operation.SendMessage (senderID, message, topics) -> 
            publishEvent (Some senderID) message topics
        | Runtime.Operation.DispatchMessage (message, topics) -> 
            publishEvent None message topics
        | Runtime.Operation.ClearRegion (owner, region) ->
            clearRegion owner region
        //| _ -> Error (UnknownOperation operation)

    and iterateStates ctx ops =
        match ops with
        | [] -> []
        | [op] -> [ processChanges ctx op ]
        | head::tail -> processChanges ctx head :: iterateStates ctx tail 

    member private this.createRuntimeView (viewHandle : ViewHandle)  (node : TreeNode) (model : obj option) (descriptor : IViewDescriptor) =
        try
            let view = (ctx.ViewRegistry |> ViewRegistry.resolve descriptor model) :?> IRuntimeView
            view.AcquireContext node descriptor this 
            view |> Ok
        with 
        | e -> View.Error.InstantiationError(viewHandle,  e) |> Runtime.Error.ViewError |> Error

    member private __.prepareView (t : Tree) (node : TreeNode) (model : obj option) (view : IRuntimeView) =
        tree <- t
        views.Add(node.Hash, view)
        match model with
        | Some m -> StateChange.ViewAddedWithModel(node, m)
        | None -> StateChange.ViewAdded(node, view.Model)
        |> changeLog.Add
        view.Load()
        (upcast view : IView)

    member private this.instantiateView (viewHandle : ViewHandle) (node : TreeNode) (model : obj option) (d : IViewDescriptor) =
        this.createRuntimeView viewHandle node model d
        |> Result.map (this.prepareView (Tree.insert node tree) node model)
        |> Result.map Runtime.Status.ViewCreated
    static member Current with get() = ForestExecutionContext._currentEngine

    static member Create (ctx : IForestContext, sp : IForestStateProvider, renderer : IPhysicalViewRenderer) = 
        let state = sp.LoadState()
        let ctx = new ForestExecutionContext(state.Tree, state.Models, state.ViewStates, state.PhysicalViews, ctx, sp, renderer)
        ForestExecutionContext._currentEngine <- ValueSome (ctx :> IForestExecutionContext)
        ctx.Init()

    member private this.Init() =
        for kvp in views do 
            let (view, n, d) = (kvp.Value, kvp.Value.InstanceID, kvp.Value.Descriptor)
            this |> view.AcquireContext n d 
        for node in (upcast tree.Hierarchy : IDictionary<_,_>).Keys do
            if not <| views.ContainsKey node.Hash then 
                let handle = node |> ViewHandle.fromNode
                match handle |> getDescriptor |> Result.bind (this.createRuntimeView handle node None) with
                | Ok view ->
                    views.Add(node.Hash, view)
                    view.Resume(models.[node.Hash])
                | _ -> ignore()
        this

    member internal this.ActivateView (node : TreeNode) =
        Runtime.Operation.InstantiateViewByNode(node, None) 
        |> this.Do 
        |> Runtime.resolve (function
            | Runtime.Status.ViewCreated view -> view
            | _ -> Unchecked.defaultof<_>
        )
    member internal this.ActivateView (node : TreeNode, model : obj) =
        Runtime.Operation.InstantiateViewByNode(node, Some model) 
        |> this.Do 
        |> Runtime.resolve (function
            | Runtime.Status.ViewCreated view -> view
            | _ -> Unchecked.defaultof<_>
        )

    member internal this.GetOrActivateView (node : TreeNode) : 'TView when 'TView :> IView =
        let result =
            match node.Hash |> views.TryGetValue with
            | (true, viewState) -> (upcast viewState : IView)
            | (false, _) -> this.ActivateView node
        downcast result:'TView

    member __.Do (operation : Runtime.Operation) =
        processChanges ctx operation

    member this.Apply (entry : StateChange) =
        match entry with
        | StateChange.ViewAddedWithModel (node, model) ->
            match TreeNode.isShell node with
            | true -> Some (Runtime.Error.SubTreeAbsent node)
            | false ->
                let hs = tree |> Tree.insert node
                match models.TryGetValue node.Hash with
                | (true, _) -> Some (Runtime.Error.SubTreeNotExpected node)
                | (false, _) ->
                    let handle = node |> ViewHandle.fromNode
                    handle
                    |> getDescriptor 
                    |> Result.bind (this.createRuntimeView handle node (Some model))
                    |> Result.map (
                        fun view ->
                            tree <- hs
                            views.Add (node.Hash, view)
                            view.Resume(model)
                        )
                    |> Result.error
        | StateChange.ViewAdded (node, model) ->
            match TreeNode.isShell node with
            | true -> Some (Runtime.Error.SubTreeAbsent node)
            | false ->
                let hs = tree |> Tree.insert node
                match models.TryGetValue node.Hash with
                | (true, _) -> Some (Runtime.Error.SubTreeNotExpected node)
                | (false, _) ->
                    let handle = node |> ViewHandle.fromNode
                    handle
                    |> getDescriptor 
                    |> Result.bind (this.createRuntimeView handle node (Some model))
                    |> Result.map (
                        fun view ->
                            tree <- hs
                            views.Add (node.Hash, view)
                            view.Resume(model)
                        )
                    |> Result.error
        | StateChange.ViewDestroyed (node) -> 
            destroyView node |> Result.error
        | StateChange.ModelUpdated (node, model) -> 
            views.[node.Hash].Resume model
            None

    member this.Dispose() = 
        ForestExecutionContext._currentEngine <- ValueNone
        for kvp in views do 
            kvp.Value.AbandonContext this
        eventBus.Dispose()
        let a, b, c, cl = this.Deconstruct()
        let intermediateState = State.create(a, b, c, pv)
        let newPv = intermediateState |> State.render ctx this renderer
        let newState = State.create(a, b, c, newPv)
            //match fuid with
            //| Some f -> State.createWithFuid(a, b, c, f)
            //| None -> State.create(a, b, c)
        sp.CommitState newState

    member internal __.Deconstruct() = 
        (
            tree, 
            models |> Seq.map (|KeyValue|) |> Map.ofSeq, 
            views |> Seq.map (|KeyValue|) |> Map.ofSeq, 
            changeLog |> List.ofSeq
        )

    //member __.Context with get() = ctx   

    member this.ExecuteCommand (NotNull "command" command) target message = 
        Runtime.Operation.InvokeCommand(command, target, message) 
        |> this.Do 
        |> Runtime.resolve ignore

    [<Obsolete>]
    member this.RegisterSystemView<'sv when 'sv :> ISystemView> () = 
        let descriptor = 
            match typeof<'sv> |> ctx.ViewRegistry.GetDescriptor |> null2vopt with
            | ValueNone -> 
                ctx.ViewRegistry
                |> ViewRegistry.registerViewType typeof<'sv> 
                |> ViewRegistry.getDescriptorByType typeof<'sv> 
            | ValueSome d -> d
        let key = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region descriptor.Name
        this.GetOrActivateView<'sv> key

    member this.SendMessage<'msg> (message : 'msg) = 
        Runtime.Operation.DispatchMessage(message, [||])
        |> this.Do
        |> Runtime.resolve ignore

    member this.LoadTree (NotNullOrEmpty "name" name) =
        name 
        |> Raw.loadTemplate ctx.TemplateProvider
        |> Templates.TemplateCompiler.compileOps
        |> Runtime.Operation.Multiple
        |> this.Do
        |> Runtime.resolve ignore

    member this.LoadTree (NotNullOrEmpty "name" name, message) =
        [Runtime.Operation.DispatchMessage(message, [||])]
        |> List.append (
            name 
            |> Raw.loadTemplate ctx.TemplateProvider
            |> Templates.TemplateCompiler.compileOps)
        |> Runtime.Operation.Multiple
        |> this.Do   
        |> Runtime.resolve ignore

    interface IForestExecutionContext with
        member __.GetViewModel node = 
            match models.TryGetValue node.Hash with
            | (true, m) -> Some m
            | (false, _) -> None

        member __.SetViewModel silent node model = 
            models.[node.Hash] <- model
            if not silent then changeLog.Add(StateChange.ModelUpdated(node, model))
            model

        member this.ActivateView(handle, region, parent) =
            Runtime.Operation.InstantiateView(handle, region, parent, None) 
            |> this.Do 
            |> Runtime.resolve (function
                | Runtime.Status.ViewCreated view -> view
                | _ -> Unchecked.defaultof<_>
            )

        member this.ActivateView(handle, region, parent, model) =
            Runtime.Operation.InstantiateView(handle, region, parent, model |> Some) 
            |> this.Do 
            |> Runtime.resolve (function
                | Runtime.Status.ViewCreated view -> view
                | _ -> Unchecked.defaultof<_>
            )

        member __.ClearRegion node region =
            clearRegion node region 
            |> Runtime.resolve ignore

        member __.GetRegionContents node region =
            getRegionContents node region 
            |> Seq.map (fun node -> (upcast views.[node.Hash] : IView))

        member __.RemoveViewFromRegion node region predicate =
            removeViewsFromRegion node region predicate 
            |> Runtime.resolve ignore

        member this.PublishEvent sender message topics = 
            Runtime.Operation.SendMessage(sender.InstanceID.Hash,message,topics) 
            |> this.Do 
            |> Runtime.resolve ignore

        member this.ExecuteCommand command issuer arg =
            Runtime.Operation.InvokeCommand(command, issuer.InstanceID.Hash, arg) 
            |> this.Do 
            |> Runtime.resolve ignore

        member __.SubscribeEvents view =
            for event in view.Descriptor.Events do
                let handler = Event.Handler(event, view)
                eventBus.Subscribe handler event.Topic |> ignore
        member __.UnsubscribeEvents view =
            eventBus.Unsubscribe view |> ignore

        //member this.Deconstruct() = this.Deconstruct()

        //member this.Context with get() = this.Context

    interface IForestEngine with
        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = this.RegisterSystemView<'sv>()
    interface IMessageDispatcher with
        member this.SendMessage<'msg> msg = this.SendMessage<'msg> msg
    interface ICommandDispatcher with
        member this.ExecuteCommand c t m = this.ExecuteCommand c t m
    interface ITreeNavigator with
        member this.LoadTree name = this.LoadTree name
        member this.LoadTree (name, msg) = this.LoadTree (name, msg)

    interface IDisposable with 
        member this.Dispose() = this.Dispose()
