namespace Forest

open Forest
open Forest.Collections
open Forest.Events
open Forest.NullHandling

open System
open System.Collections.Generic

[<RequireQualifiedAccess>]
module Runtime =
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [<System.Serializable>]
    #endif
    [<CompiledName("Operation")>]
    type [<Struct>] Operation =
        | InstantiateView of node:TreeNode
        | UpdateViewModel of parent:hash * viewModel:obj
        | DestroyView of subtree:TreeNode
        | InvokeCommand of owner:hash * commandName:cname * commandArg:obj
        | PublishEvent of senderID:hash * message:obj * topics:string array
        | Multiple of operations:Operation list

    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [<Serializable>]
    #endif
    [<CompiledName("Error")>]
    type [<Struct>] Error =
        | ViewstateAbsent of view:vname
        | UnexpectedState of node:TreeNode
        | CommandError of commandErrorCause:Command.Error
        | ViewError of viewErrorCause:View.Error
        | SubTreeAbsent of orphanIdentifier:TreeNode

    let resolveError (error:Error) =
        match error with
        | ViewError ve -> ve |> View.resolveError 
        | CommandError ce -> ce |> Command.resolveError 
        // TODO
        | _ -> ()

type [<Sealed>] internal ForestRuntime private (t:Tree, models:Map<hash, obj>, views:Map<hash, IRuntimeView>, ctx:IForestContext) as self = 
    let mutable tree = t
    let eventBus:IEventBus = Event.createEventBus()
    let models:System.Collections.Generic.Dictionary<hash, obj> = System.Collections.Generic.Dictionary(models, StringComparer.Ordinal)
    let views:System.Collections.Generic.Dictionary<hash, IRuntimeView> = System.Collections.Generic.Dictionary(views, StringComparer.Ordinal)
    let changeLog:System.Collections.Generic.List<StateChange> = System.Collections.Generic.List()

    let updateViewModel (id:hash) (vm:obj) : Result<unit, Runtime.Error> =
        models.[id] <- vm
        Ok ()
    let destroyView (node:TreeNode) : Result<unit, Runtime.Error> =
        let (t, nodes) = Tree.remove node tree
        for removedNode in nodes do
            let removedHash = removedNode.Hash
            models.Remove removedHash |> ignore
            views.Remove removedHash |> ignore
            changeLog.Add(StateChange.ViewDestroyed(removedNode))
        tree <- t
        Ok ()
    let executeCommand (stateKey:hash) (name:cname) (arg:obj) : Result<unit, Runtime.Error> =
        match views.TryGetValue stateKey with
        | (true, view) ->
            match view.Descriptor.Commands.TryFind name with
            | Some cmd -> 
                view |> cmd.Invoke arg
                Ok ()
            | None ->  Error (Runtime.Error.CommandError <| Command.Error.CommandNotFound(view.Descriptor.ViewType, name))
        | (false, _) -> (Error (Runtime.Error.ViewstateAbsent stateKey))
    let publishEvent (id:hash) (message:'m) (topics:string array) : Result<unit, Runtime.Error> =
        match views.TryGetValue id with
        | (true, sender) -> 
            eventBus.Publish(sender, message, topics)
            Ok ()
        | _ -> Ok ()
    let rec processChanges (ctx:IForestContext) (operation:Runtime.Operation) =
        match operation with
        | Runtime.Operation.Multiple operations -> 
            iterateStates ctx operations
        | Runtime.Operation.InstantiateView (node) -> 
            self.instantiateView node
        | Runtime.Operation.UpdateViewModel (viewID, model) -> 
            updateViewModel viewID model
        | Runtime.Operation.DestroyView viewID -> 
            destroyView viewID
        | Runtime.Operation.InvokeCommand (viewID, commandName, arg) -> 
            executeCommand viewID commandName arg
        | Runtime.Operation.PublishEvent (senderID, message, topics) -> 
            publishEvent senderID message topics
        //| _ -> Error (UnknownOperation operation)
    and iterateStates ctx ops =
        match ops with
        | [] -> Ok ()
        | [op] -> processChanges ctx op
        | head::tail ->
            processChanges ctx head
            |> Result.map (fun _ -> tail)
            |> Result.bind (iterateStates ctx)
    member private this.createViewState(node:TreeNode) =
        match ctx.ViewRegistry.GetDescriptor(node.View) |> null2vopt with
        | ValueSome vd ->
            let view = (ctx.ViewRegistry.Resolve node.View) :?> IRuntimeView
            view.InstanceID <- node
            view.Descriptor <- vd
            view.AcquireRuntime this
            Ok view // will also set the view model
        | ValueNone -> 
            // this is a different error
            Error (Runtime.Error.ViewstateAbsent node.Hash)
    member private this.instantiateView (node:TreeNode) : Result<unit, Runtime.Error> =
        let t = (Tree.insert node tree)
        match this.createViewState node with
        | Ok view ->
            tree <- t
            views.Add(node.Hash, view)
            changeLog.Add(StateChange.ViewAdded(node, view.ViewModel))
            view.Load()
            Ok ()
        | Error e -> Error e
    static member Create (tree:Tree, models:Map<hash, obj>, views:Map<hash, IRuntimeView>, ctx:IForestContext) = 
        (new ForestRuntime(tree, models, views, ctx)).Init()
    member private this.Init() =
        for kvp in views do 
            kvp.Value.AcquireRuntime this
        for node in (upcast tree.Hierarchy:IDictionary<_,_>).Keys do
            if not <| views.ContainsKey node.Hash then 
                match this.createViewState node with
                | Ok view ->
                    views.Add(node.Hash, view)
                    view.Resume(models.[node.Hash])
                | _ -> ignore()
        this
    member internal this.ActivateView(node) =
        Runtime.Operation.InstantiateView(node) |> this.Update |> ignore
        match views.TryGetValue node.Hash with
        | (true, viewState) -> (upcast viewState:IView)
        | (false, _) -> nil<_>
    member internal this.ActivateAnonymousView<'v when 'v:>IView> parent region =
        let d = ctx.ViewRegistry.GetDescriptor typeof<'v>
        let view =
            if (d.Name |> String.IsNullOrEmpty |> not) then
                let node = TreeNode.newKey region d.Name parent
                this.ActivateView node
            else ctx.ViewRegistry.Resolve typeof<'v>
        downcast view:'v
    member internal this.GetOrActivateView (node:TreeNode) : 'TView when 'TView :> IView =
        let result =
            match node.Hash |> views.TryGetValue with
            | (true, viewState) -> (upcast viewState:IView)
            | (false, _) -> this.ActivateView node
        downcast result:'TView
    member __.Update (operation:Runtime.Operation) =
        match processChanges ctx operation with
        | Ok _ -> ()
        | Error e -> Runtime.resolveError e
    member this.Apply (entry:StateChange) =
        match entry with
        | StateChange.ViewAdded (node, model) ->
            match TreeNode.isShell node with
            | true -> Some (Runtime.Error.SubTreeAbsent node)
            | false ->
                let hs = tree |> Tree.insert node
                match models.TryGetValue node.Hash with
                | (true, _) -> Some (Runtime.Error.UnexpectedState node)
                | (false, _) ->
                    match this.createViewState node with
                    | Ok view ->
                        tree <- hs
                        views.Add (node.Hash, view)
                        view.Resume(model)
                        None
                    | Error e -> Some e
        | StateChange.ViewDestroyed (node) ->
            match destroyView node with Ok _ -> None | Error e -> Some e
        | StateChange.ViewModelUpdated (node, model) -> 
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
            if not silent then changeLog.Add(StateChange.ViewModelUpdated(node, model))
            model
        member this.ActivateView parent region name =
            let node = parent |> TreeNode.newKey region name 
            this.ActivateView node
        member this.ActivateAnonymousView<'v when 'v:>IView> parent region =
            this.ActivateAnonymousView<'v> parent region
        member this.PublishEvent sender message topics = 
            Runtime.Operation.PublishEvent(sender.InstanceID.Hash,message,topics) |> this.Update |> ignore
        member this.ExecuteCommand issuer command arg =
            Runtime.Operation.InvokeCommand(issuer.InstanceID.Hash, command, arg) |> this.Update |> ignore
        member __.SubscribeEvents view =
            for event in view.Descriptor.Events do
                let handler = Event.Handler(event, view)
                eventBus.Subscribe handler event.Topic |> ignore
        member __.UnsubscribeEvents view =
            eventBus.Unsubscribe view |> ignore
    interface IDisposable with 
        member this.Dispose() = 
            this.Dispose()
