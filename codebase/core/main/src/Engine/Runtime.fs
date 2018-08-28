namespace Forest

open Forest
open Forest.Events
open Forest.NullHandling

open System
open System.Collections.Generic

[<RequireQualifiedAccess>]
module Runtime =
    [<Serializable>]
    [<CompiledName("Error")>]
    type [<Struct>] Error =
        | ViewstateAbsent of view:vname
        | UnexpectedState of identifier:HierarchyKey
        | CommandError of commandErrorCause:Command.Error
        | ViewError of viewErrorCause:View.Error
        | HierarchyElementAbsent of orphanIdentifier:HierarchyKey

    [<Serializable>]
    [<CompiledName("Operation")>]
    type [<Struct>] Operation =
        | InstantiateView of id:HierarchyKey
        | UpdateViewModel of parent:sname * viewModel:obj
        | DestroyView of identifier:HierarchyKey
        | InvokeCommand of owner:sname * commandName:cname * commandArg:obj
        | PublishEvent of senderID:sname * message:obj * topics:string array
        | Multiple of operations:Operation list

    let resolveError (error:Error) =
        match error with
        | ViewError ve -> ve |> View.resolveError 
        | CommandError ce -> ce |> Command.resolveError 
        // TODO
        | _ -> ()

type [<Sealed>] internal ForestRuntime private (hierarchy:Hierarchy, viewModels:Map<sname, obj>, viewStates:Map<sname, IViewState>, ctx:IForestContext) as self = 
    let mutable hierarchy = hierarchy
    let eventBus:IEventBus = Event.createEventBus()
    let viewModels:System.Collections.Generic.Dictionary<sname, obj> = System.Collections.Generic.Dictionary(viewModels, StringComparer.Ordinal)
    let viewStates:System.Collections.Generic.Dictionary<sname, IViewState> = System.Collections.Generic.Dictionary(viewStates, StringComparer.Ordinal)
    let changeLog:System.Collections.Generic.List<StateChange> = System.Collections.Generic.List()

    let updateViewModel (id:sname) (vm:obj) : Result<unit, Runtime.Error> =
        viewModels.[id] <- vm
        Ok ()
    let destroyView (id:HierarchyKey) : Result<unit, Runtime.Error> =
        let (h, ids) = Hierarchy.remove id hierarchy
        for removedID in ids do
            let removedIDHash = removedID.Hash
            viewModels.Remove removedIDHash |> ignore
            viewStates.Remove removedIDHash |> ignore
            changeLog.Add(StateChange.ViewDestroyed(removedID))
        hierarchy <- h
        Ok ()
    let executeCommand (stateKey:sname) (name:cname) (arg:obj) : Result<unit, Runtime.Error> =
        match viewStates.TryGetValue stateKey with
        | (true, vs) ->
            match vs.Descriptor.Commands.TryFind name with
            | Some cmd -> 
                vs |> cmd.Invoke arg
                Ok ()
            | None ->  Error (Runtime.Error.CommandError <| Command.Error.CommandNotFound(vs.Descriptor.ViewType, name))
        | (false, _) -> (Error (Runtime.Error.ViewstateAbsent stateKey))
    let publishEvent (id:sname) (message:'m) (topics:string array) : Result<unit, Runtime.Error> =
        match viewStates.TryGetValue id with
        | (true, sender) -> 
            eventBus.Publish(sender, message, topics)
            Ok ()
        | _ -> Ok ()
    let rec processChanges (ctx:IForestContext) (operation:Runtime.Operation) =
        match operation with
        | Runtime.Operation.Multiple operations -> 
            iterateStates ctx operations
        | Runtime.Operation.InstantiateView (id) -> 
            self.addViewState id
        | Runtime.Operation.UpdateViewModel (viewID, vm) -> 
            updateViewModel viewID vm
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
    member private this.createViewState(id:HierarchyKey) =
        match ctx.ViewRegistry.GetDescriptor(id.View) |> null2vopt with
        | ValueSome vd ->
            let vi = (ctx.ViewRegistry.Resolve id.View) :?> IViewState
            vi.InstanceID <- id
            vi.Descriptor <- vd
            vi.AcquireRuntime this
            Ok vi // will also set the view model
        | ValueNone -> 
            // this is a different error
            Error (Runtime.Error.ViewstateAbsent id.Hash)
    member private this.addViewState (id:HierarchyKey) : Result<unit, Runtime.Error> =
        let hs = (Hierarchy.insert id hierarchy)
        match this.createViewState id with
        | Ok viewState ->
            hierarchy <- hs
            viewStates.Add(id.Hash, viewState)
            changeLog.Add(StateChange.ViewAdded(id, viewState.ViewModel))
            viewState.Load()
            Ok ()
        | Error e -> Error e
    static member Create (hierarchy:Hierarchy, viewModels:Map<sname, obj>, viewStates:Map<sname, IViewState>, ctx:IForestContext) = 
        (new ForestRuntime(hierarchy, viewModels, viewStates, ctx)).Init()
    member private this.Init() =
        for kvp in viewStates do 
            kvp.Value.AcquireRuntime this
        for id in (upcast hierarchy.Hierarchy:IDictionary<_,_>).Keys do
            if not <| viewStates.ContainsKey id.Hash then 
                match this.createViewState id with
                | Ok viewState ->
                    viewStates.Add(id.Hash, viewState)
                    viewState.Resume(viewModels.[id.Hash])
                | _ -> ignore()
        this
    member internal this.ActivateView(id) =
        Runtime.Operation.InstantiateView(id) |> this.Update |> ignore
        match viewStates.TryGetValue id.Hash with
        | (true, viewState) -> (upcast viewState:IView)
        | (false, _) -> nil<_>
    member internal this.ActivateAnonymousView<'v when 'v:>IView> parent region =
        let d = ctx.ViewRegistry.GetDescriptor typeof<'v>
        let view =
            if (d.Name |> String.IsNullOrEmpty |> not) then
                let id = HierarchyKey.newKey region d.Name parent
                this.ActivateView id
            else ctx.ViewRegistry.Resolve typeof<'v>
        downcast view:'v
    member internal this.GetOrActivateView (id:HierarchyKey) : 'TView when 'TView :> IView =
        let result =
            match id.Hash |> viewStates.TryGetValue with
            | (true, viewState) -> (upcast viewState:IView)
            | (false, _) -> this.ActivateView id
        downcast result:'TView
    member this.Update (operation:Runtime.Operation) =
        match processChanges ctx operation with
        | Ok _ -> ()
        | Error e -> Runtime.resolveError e
    member this.Apply (entry:StateChange) =
        match entry with
        | StateChange.ViewAdded (id, vm) ->
            match HierarchyKey.isShell id with
            | true -> Some (Runtime.Error.HierarchyElementAbsent(id))
            | false ->
                let hs = hierarchy |> Hierarchy.insert id
                match viewModels.TryGetValue id.Hash with
                | (true, _) -> Some (Runtime.Error.UnexpectedState id)
                | (false, _) ->
                    match this.createViewState id with
                    | Ok viewState ->
                        hierarchy <- hs
                        viewStates.Add (id.Hash, viewState)
                        viewState.Resume(vm)
                        None
                    | Error e -> Some e
        | StateChange.ViewDestroyed (id) ->
            match destroyView id with Ok _ -> None | Error e -> Some e
        | StateChange.ViewModelUpdated (id, vm) -> 
            viewStates.[id.Hash].Resume(vm)
            None
    member this.Dispose() = 
        for kvp in viewStates do 
            kvp.Value.AbandonRuntime this
        eventBus.Dispose()
    member internal __.Deconstruct() = 
        (
            hierarchy, 
            viewModels |> Seq.map (|KeyValue|) |> Map.ofSeq, 
            viewStates |> Seq.map (|KeyValue|) |> Map.ofSeq, 
            changeLog |> List.ofSeq
        )
    member __.Context with get() = ctx

    interface IForestRuntime with
        member __.GetViewModel id = 
            match viewModels.TryGetValue id.Hash with
            | (true, v) -> Some v
            | (false, _) -> None
        member __.SetViewModel silent id vm = 
            viewModels.[id.Hash] <- vm
            if not silent then changeLog.Add(StateChange.ViewModelUpdated(id, vm))
            vm
        member this.ActivateView parent region name =
            let id = parent |> HierarchyKey.newKey region name 
            this.ActivateView id
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
