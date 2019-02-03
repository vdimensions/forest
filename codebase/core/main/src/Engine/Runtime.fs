namespace Forest
open System
open System.Collections.Generic
open Axle.Option
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
        | InstantiateView of node : TreeNode
        | InstantiateViewWithModel of node : TreeNode * model : obj
        | InstantiateAnonymousView of viewType : Type
        | InstantiateAnonymousViewWithModel of viewType : Type * model : obj
        | UpdateModel of node : thash * model : obj
        | DestroyView of subtree : TreeNode
        | InvokeCommand of command : cname * node : thash * commandArg : obj
        | PublishEvent of node : thash * message : obj * topics : string array
        | ClearRegion of owner : TreeNode * region : rname
        | Multiple of operations : Operation list

    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [<Serializable>]
    #endif
    [<CompiledName("Error")>]
    [<NoComparison>] 
    type Error =
        /// Error when the view that is a target of a command or message is not found
        | ViewstateAbsent of hash : thash
        /// Unable to find a descriptor for the given view type
        | NoDescriptor of viewName : vname
        /// A tree node was found to be already present in the hierarchy at a place where it was going to be created
        | SubTreeNotExpected of node : TreeNode
        /// A tree node was expected to be present in the hierarchy but was not found
        | SubTreeAbsent of node : TreeNode
        | CommandError of cause : Command.Error
        | ViewError of cause : View.Error
        | Multiple of errors : Error list

    let private resolveError (error : Error) =
        match error with
        | ViewError ve -> ve |> View.resolveError 
        | CommandError ce -> ce |> Command.resolveError 
        // TODO
        | _ -> ()

    let resolve<'t> (result : Result<'t, Error>) =
        match result with
        | Ok x -> x |> ignore
        | Error e -> e |> resolveError

[<Sealed>]
[<NoComparison>]
type internal ForestRuntime private (t : Tree, models : Map<thash, obj>, views : Map<thash, IRuntimeView>, ctx : IForestContext) as self = 
    let mutable tree = t
    let eventBus : IEventBus = Event.createEventBus()
    let models : System.Collections.Generic.Dictionary<thash, obj> = System.Collections.Generic.Dictionary(models, StringComparer.Ordinal)
    let views : System.Collections.Generic.Dictionary<thash, IRuntimeView> = System.Collections.Generic.Dictionary(views, StringComparer.Ordinal)
    let changeLog : System.Collections.Generic.List<StateChange> = System.Collections.Generic.List()

    let viewDescriptorNotFoundError (viewType : Type) =
        invalidOp <| String.Format("Unable to obtain descriptor for view {0}", viewType.AssemblyQualifiedName)

    let updateModel (id : thash) (m : obj) : Result<unit, Runtime.Error> =
        models.[id] <- m
        Ok ()

    let destroyView (node : TreeNode) : Result<unit, Runtime.Error> =
        let (t, nodes) = tree |> Tree.remove node
        for removedNode in nodes do
            let removedHash = removedNode.Hash
            let view = views.[removedHash]
            view.Dispose()
            models.Remove removedHash |> ignore
            views.Remove removedHash |> ignore
            changeLog.Add(StateChange.ViewDestroyed(removedNode))
        tree <- t
        Ok ()

    let getRegionContents (owner : TreeNode) (region : rname) =
        let isInRegion (node:TreeNode) = StringComparer.Ordinal.Equals(region, node.Region)
        tree |> Tree.filter owner isInRegion

    let clearRegion (owner : TreeNode) (region : rname) =
        let mutable errors = List.empty<Runtime.Error>
        for child in getRegionContents owner region do
            errors <- 
                match destroyView child with
                | Ok _ -> errors
                | Error e -> e::errors
        match errors with
        | [] -> Ok()
        | list -> Error <| Runtime.Error.Multiple list

    let removeViewsFromRegion (owner : TreeNode) (region : rname) (predicate : System.Predicate<IView>)=
        let mutable errors = List.empty<Runtime.Error>
        for child in getRegionContents owner region do
            if (predicate.Invoke(views.[child.Hash])) then
                errors <- 
                    match destroyView child with
                    | Ok _ -> errors
                    | Error e -> e::errors
        match errors with
        | [] -> Ok()
        | list -> Error <| Runtime.Error.Multiple list

    let executeCommand (name : cname) (stateKey : thash) (arg : obj) : Result<unit, Runtime.Error> =
        match views.TryGetValue stateKey with
        | (true, view) ->
            match view.Descriptor.Commands.TryFind name with
            | Some cmd -> 
                view |> cmd.Invoke arg
                Ok ()
            | None ->  Error (Runtime.Error.CommandError <| Command.Error.CommandNotFound(view.Descriptor.ViewType, name))
        | (false, _) -> (Error (Runtime.Error.ViewstateAbsent stateKey))

    let publishEvent (id : thash) (message : 'm) (topics : string array) : Result<unit, Runtime.Error> =
        match views.TryGetValue id with
        | (true, sender) -> 
            eventBus.Publish(sender, message, topics)
            Ok ()
        | _ -> Ok ()

    let rec processChanges (ctx : IForestContext) (operation : Runtime.Operation) =
        match operation with
        | Runtime.Operation.Multiple operations -> 
            iterateStates ctx operations
        | Runtime.Operation.InstantiateView (node) -> 
            self.instantiateView node None
        | Runtime.Operation.InstantiateViewWithModel (node, model) -> 
            self.instantiateView node (Some model)
        | Runtime.Operation.UpdateModel (viewID, model) -> 
            updateModel viewID model
        | Runtime.Operation.DestroyView viewID -> 
            destroyView viewID
        | Runtime.Operation.InvokeCommand (commandName, viewID, arg) -> 
            executeCommand commandName viewID arg
        | Runtime.Operation.PublishEvent (senderID, message, topics) -> 
            publishEvent senderID message topics
        | Runtime.Operation.ClearRegion (owner, region) ->
            clearRegion owner region
        //| _ -> Error (UnknownOperation operation)

    and iterateStates ctx ops =
        match ops with
        | [] -> Ok ()
        | [op] -> processChanges ctx op
        | head::tail ->
            processChanges ctx head
            |> Result.map (fun _ -> tail)
            |> Result.bind (iterateStates ctx)

    member private this.createView(node : TreeNode) (model : obj option) =
        match ctx.ViewRegistry.GetDescriptor(node.View) |> null2vopt with
        | ValueSome vd ->
            let view =
                match model with
                | Some model -> (ctx.ViewRegistry.Resolve(node.View, model)) :?> IRuntimeView
                | None -> (ctx.ViewRegistry.Resolve node.View) :?> IRuntimeView
            view.AcquireRuntime node vd this 
            Ok view // will also set the view model
        | ValueNone -> 
            // this is a different error
            Error (Runtime.Error.NoDescriptor (node.View))

    member private this.instantiateView (node : TreeNode) (model : obj option) : Result<unit, Runtime.Error> =
        let t = (Tree.insert node tree)
        match this.createView node model with
        | Ok view ->
            tree <- t
            views.Add(node.Hash, view)
            match model with
            | Some m -> StateChange.ViewAddedWithModel(node, m)
            | None -> StateChange.ViewAdded(node, view.Model)
            |> changeLog.Add
            view.Load()
            Ok ()
        | Error e -> Error e

    static member Create (tree : Tree, models : Map<thash, obj>, views : Map<thash, IRuntimeView>, ctx : IForestContext) = 
        (new ForestRuntime(tree, models, views, ctx)).Init()

    member private this.Init() =
        for kvp in views do 
            let (view, n, d) = (kvp.Value, kvp.Value.InstanceID, kvp.Value.Descriptor)
            this |> view.AcquireRuntime n d 
        for node in (upcast tree.Hierarchy:IDictionary<_,_>).Keys do
            if not <| views.ContainsKey node.Hash then 
                match this.createView node None with
                | Ok view ->
                    views.Add(node.Hash, view)
                    view.Resume(models.[node.Hash])
                | _ -> ignore()
        this

    member internal this.ActivateView (node) =
        Runtime.Operation.InstantiateView(node) |> this.Update |> ignore
        match views.TryGetValue node.Hash with
        | (true, view) -> (upcast view : IView)
        | (false, _) -> Unchecked.defaultof<_>

    member internal this.ActivateView (model : 'm, node) =
        Runtime.Operation.InstantiateViewWithModel(node, model) |> this.Update |> ignore
        match views.TryGetValue node.Hash with
        | (true, view) -> (downcast view : IView<'m>)
        | (false, _) -> Unchecked.defaultof<_>

    member internal this.ActivateAnonymousView<'v when 'v :> IView>(region, parent) =
        let d = ctx.ViewRegistry.GetDescriptor typeof<'v>
        match null2opt d with
        | Some d ->
            let view =
                if (d.Name |> String.IsNullOrEmpty |> not) then
                    let node = TreeNode.newKey region d.Name parent
                    this.ActivateView node
                else ctx.ViewRegistry.Resolve typeof<'v>
            downcast view:'v
        | None -> viewDescriptorNotFoundError typeof<'v>

    member internal this.ActivateAnonymousView<'v, 'm when 'v :> IView<'m>>(model : 'm, region, parent) =
        let d = ctx.ViewRegistry.GetDescriptor typeof<'v>
        match null2opt d with
        | Some d ->
            let view =
                if (d.Name |> String.IsNullOrEmpty |> not) then
                    let node = TreeNode.newKey region d.Name parent
                    downcast this.ActivateView(model, node) : 'v
                else downcast ctx.ViewRegistry.Resolve typeof<'v> : 'v
            view
        | None -> viewDescriptorNotFoundError typeof<'v>

    member internal this.GetOrActivateView (node : TreeNode) : 'TView when 'TView :> IView =
        let result =
            match node.Hash |> views.TryGetValue with
            | (true, viewState) -> (upcast viewState:IView)
            | (false, _) -> this.ActivateView node
        downcast result:'TView

    member __.Update (operation : Runtime.Operation) =
        processChanges ctx operation |> Runtime.resolve

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
                    match this.createView node (Some model) with
                    | Ok view ->
                        tree <- hs
                        views.Add (node.Hash, view)
                        view.Resume(model)
                        None
                    | Error e -> Some e
        | StateChange.ViewAdded (node, model) ->
            match TreeNode.isShell node with
            | true -> Some (Runtime.Error.SubTreeAbsent node)
            | false ->
                let hs = tree |> Tree.insert node
                match models.TryGetValue node.Hash with
                | (true, _) -> Some (Runtime.Error.SubTreeNotExpected node)
                | (false, _) ->
                    match this.createView node None with
                    | Ok view ->
                        tree <- hs
                        views.Add (node.Hash, view)
                        view.Resume(model)
                        None
                    | Error e -> Some e
        | StateChange.ViewDestroyed (node) ->
            match destroyView node with Ok _ -> None | Error e -> Some e
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

        member this.ActivateView(name, region, parent) =
            let node = parent |> TreeNode.newKey region name 
            this.ActivateView node

        member this.ActivateView(model : 'm, name, region, parent) =
            let node = parent |> TreeNode.newKey region name 
            this.ActivateView(model, node)

        member this.ActivateAnonymousView<'v when 'v :> IView>(region, parent) =
            this.ActivateAnonymousView<'v>(region, parent)

        member this.ActivateAnonymousView<'v, 'm when 'v :> IView<'m>>(model, region, parent) =
            this.ActivateAnonymousView<'v, 'm>(model, region, parent)

        member __.ClearRegion node region =
            clearRegion node region |> Runtime.resolve

        member __.GetRegionContents node region =
            getRegionContents node region 
            |> Seq.map (fun node -> (upcast views.[node.Hash] : IView))

        member __.RemoveViewFromRegion node region predicate =
            removeViewsFromRegion node region predicate |> Runtime.resolve

        member this.PublishEvent sender message topics = 
            Runtime.Operation.PublishEvent(sender.InstanceID.Hash,message,topics) |> this.Update

        member this.ExecuteCommand command issuer arg =
            Runtime.Operation.InvokeCommand(command, issuer.InstanceID.Hash, arg) |> this.Update

        member __.SubscribeEvents view =
            for event in view.Descriptor.Events do
                let handler = Event.Handler(event, view)
                eventBus.Subscribe handler event.Topic |> ignore

        member __.UnsubscribeEvents view =
            eventBus.Unsubscribe view |> ignore

    interface IDisposable with 
        member this.Dispose() = this.Dispose()
