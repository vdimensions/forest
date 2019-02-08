namespace Forest
open System
open System.Collections.Generic
open Axle
open Forest
open Forest.Collections
open Forest.Events

[<RequireQualifiedAccess>]
module Runtime =
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [<System.Serializable>]
    #endif
    [<CompiledName("Operation")>]
    [<NoComparison>]
    type Operation =
        | InstantiateView of viewHandle : ViewHandle * region : rname * parent : TreeNode * model : obj option
        | InstantiateViewByNode of node : TreeNode * model : obj option
        | UpdateModel of node : thash * model : obj
        | DestroyView of subtree : TreeNode
        | InvokeCommand of command : cname * node : thash * commandArg : obj
        | SendMessage of node : thash * message : obj * topics : string array
        | ClearRegion of owner : TreeNode * region : rname
        | Multiple of operations : Operation list

    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [<System.Serializable>]
    #endif
    [<NoComparison>]
    type Status =
        | ViewCreated of view : IView
        | ModelUpdated of model : obj
        | ViewDestoyed
        | CommandInvoked
        | MesssagePublished
        | MessageSourceNotFound
        | RegionCleared
        | Multiple of Status list

    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [<Serializable>]
    #endif
    [<NoComparison>] 
    type Error =
        /// Error when the view that is a target of a command or message is not found
        | ViewstateAbsent of hash : thash
        /// Unable to find a descriptor for the given view type
        | NoDescriptor of viewHandle : ViewHandle
        /// A tree node was found to be already present in the hierarchy at a place where it was going to be created
        | SubTreeNotExpected of node : TreeNode
        /// A tree node was expected to be present in the hierarchy but was not found
        | SubTreeAbsent of node : TreeNode
        | CommandError of cause : Command.Error
        | ViewError of cause : View.Error
        | Multiple of errors : Error list

    let private handleError (error : Error) =
        match error with
        | ViewError ve -> ve |> View.handleError 
        | CommandError ce -> ce |> Command.handleError 
        | NoDescriptor vh -> 
            match vh with
            | ByName vn -> invalidOp <| String.Format("Unable to obtain descriptor for view '{0}'", vn)
            | ByType vt -> invalidOp <| String.Format("Unable to obtain descriptor for view `{0}`", vt.AssemblyQualifiedName)
        // TODO
        | _ -> ()

    let resolve (resultMap : ('x -> 'a)) (result : Result<'x, Error>) =
        match result with
        | Ok x -> x |> resultMap
        | Error e -> e |> handleError; invalidOp "An unknown error occurred"

[<Sealed>]
[<NoComparison>]
type internal ForestRuntime private (t : Tree, models : Map<thash, obj>, views : Map<thash, IRuntimeView>, ctx : IForestContext) as self = 
    let mutable tree = t
    let eventBus : IEventBus = Event.createEventBus()
    let models : System.Collections.Generic.Dictionary<thash, obj> = System.Collections.Generic.Dictionary(models, StringComparer.Ordinal)
    let views : System.Collections.Generic.Dictionary<thash, IRuntimeView> = System.Collections.Generic.Dictionary(views, StringComparer.Ordinal)
    let changeLog : System.Collections.Generic.List<StateChange> = System.Collections.Generic.List()

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

    let publishEvent (id : thash) (message : 'm) (topics : string array) =
        match views.TryGetValue id with
        | (true, sender) -> 
            eventBus.Publish(sender, message, topics)
            Runtime.Status.MesssagePublished |> Ok
        // TODO: should be errror
        | _ -> Runtime.Status.MessageSourceNotFound |> Ok

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
            node
            |> ViewHandle.fromNode
            |> getDescriptor
            |> Result.bind (
                fun descriptor -> 
                    self.instantiateView (ViewHandle.fromNode node) node model descriptor
                )
        | Runtime.Operation.UpdateModel (viewID, model) -> 
            updateModel viewID model
        | Runtime.Operation.DestroyView viewID -> 
            destroyView viewID
        | Runtime.Operation.InvokeCommand (commandName, viewID, arg) -> 
            executeCommand commandName viewID arg
        | Runtime.Operation.SendMessage (senderID, message, topics) -> 
            publishEvent senderID message topics
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
            view.AcquireRuntime node descriptor this 
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

    static member Create (tree : Tree, models : Map<thash, obj>, views : Map<thash, IRuntimeView>, ctx : IForestContext) = 
        (new ForestRuntime(tree, models, views, ctx)).Init()

    member private this.Init() =
        for kvp in views do 
            let (view, n, d) = (kvp.Value, kvp.Value.InstanceID, kvp.Value.Descriptor)
            this |> view.AcquireRuntime n d 
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
        for kvp in views do 
            kvp.Value.AbandonRuntime this
        eventBus.Dispose()

    member internal __.Deconstruct() = 
        (
            tree, 
            models |> Seq.map (|KeyValue|) |> Map.ofSeq, 
            views |> Seq.map (|KeyValue|) |> Map.ofSeq, 
            changeLog |> List.ofSeq
        )
    member __.Context with get() = ctx

    interface IForestRuntime with
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

    interface IDisposable with 
        member this.Dispose() = this.Dispose()
