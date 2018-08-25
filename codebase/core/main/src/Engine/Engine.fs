namespace Forest

open Forest
open Forest.Events
open Forest.NullHandling

open System


type [<Sealed>] EngineResult internal (state:State, changeList:ChangeList) = 
    do
        ignore <| isNotNull "state" state
        ignore <| isNotNull "changeList" changeList
    member __.State with get() = state
    member __.ChangeList with get() = changeList

[<Serializable>]
type [<Struct>] ForestOperation =
    | InstantiateView of id:HierarchyKey
    | UpdateViewModel of parent:HierarchyKey * viewModel:obj
    | DestroyView of identifier:HierarchyKey
    | InvokeCommand of owner:HierarchyKey * commandName:string * commandArg:obj
    | PublishEvent of senderID:HierarchyKey * message:obj * topics:string array
    | Multiple of operations: ForestOperation list

//type [<Sealed>] private ScopeContextRegistry(ctx:IForestContext) =
//    
//    interface IViewRegistry with
//        member Register: t:Type =
//        member Register<'T when 'T:> IView> : unit =
//        member Resolve: name:string -> IView
//        member GetDescriptor: name:string -> IViewDescriptor
//type [<Sealed>] internal ScopeContext
    

type [<Sealed>] internal MutableScope private (hierarchy:Hierarchy, viewModels:Map<HierarchyKey, obj>, viewStates:Map<HierarchyKey, IViewState>, ctx:IForestContext) as self = 
    let mutable hierarchy = hierarchy
    let eventBus:IEventBus = Event.createEventBus()
    let viewModels:System.Collections.Generic.Dictionary<HierarchyKey, obj> = System.Collections.Generic.Dictionary(viewModels)
    let viewStates:System.Collections.Generic.Dictionary<HierarchyKey, IViewState> = System.Collections.Generic.Dictionary(viewStates)

    let _toList (l: System.Collections.Generic.IList<'a>) = List.ofSeq l
    let _toMap (dict: System.Collections.Generic.IDictionary<'a, 'b>) : Map<'a, 'b> =
        dict
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq

    let changeLog: System.Collections.Generic.List<StateChange> = System.Collections.Generic.List()

    let updateViewModel (id: HierarchyKey) (vm: obj): Result<StateChange List, StateError> =
        viewModels.[id] <- vm
        Ok (_toList changeLog)

    let destroyView (id: HierarchyKey): Result<StateChange List, StateError> =
        let (h, ids) = Hierarchy.remove id hierarchy
        for removedID in ids do
            viewModels.Remove removedID |> ignore
            viewStates.Remove removedID |> ignore
            changeLog.Add(StateChange.ViewDestroyed(removedID))
        hierarchy <- h
        Ok (_toList changeLog)

    let executeCommand (id: HierarchyKey) (name: string) (arg: obj) : Result<StateChange List, StateError> =
        match viewStates.TryGetValue id with
        | (true, vs) ->
            match vs.Descriptor.Commands.TryFind name with
            | Some cmd -> 
                vs |> cmd.Invoke arg
                Ok (_toList changeLog)
            | None ->  Error (CommandNotFound(id, name))
        | (false, _) -> (Error (ViewNotFound id.View))

    let publishEvent (id:HierarchyKey) (message:'m) (topics:string array) : Result<StateChange List, StateError> =
        match viewStates.TryGetValue id with
        | (true, sender) -> 
            eventBus.Publish(sender, message, topics)
            Ok (_toList changeLog)
        | _ -> Ok (_toList changeLog)

    let rec processChanges (ctx: IForestContext) (operation: ForestOperation) =
        match operation with
        | Multiple operations -> _loopStates ctx operations
        | InstantiateView (id) -> self.addViewState ctx id |> ignore
        | UpdateViewModel (viewID, vm) -> updateViewModel viewID vm |> ignore
        | DestroyView viewID -> destroyView viewID |> ignore
        | InvokeCommand (viewID, commandName, arg) -> executeCommand viewID commandName arg |> ignore
        | PublishEvent (senderID, message, topics) -> publishEvent senderID message topics |> ignore
        //| _ -> Error (UnknownOperation operation)
    and _loopStates ctx ops =
        match ops with
        | [] -> ()
        | [op] -> processChanges ctx op
        | head::tail ->
            processChanges ctx head
            _loopStates ctx tail

    member private this.createViewState(ctx: IForestContext) (id: HierarchyKey) =
        match ctx.ViewRegistry.GetDescriptor(id.View) |> null2vopt with
        | ValueSome vd ->
            let vi = (ctx.ViewRegistry.Resolve id.View) :?> IViewState
            vi.InstanceID <- id
            vi.Descriptor <- vd
            vi.EnterModificationScope this
            Ok vi // will also set the view model
        | ValueNone -> Error (ViewNotFound id.View)

    member private this.unvisitViewState (vs:IViewState) =
        vs.LeaveModificationScope this

    member private this.addViewState (ctx: IForestContext) (id: HierarchyKey): Result<StateChange List, StateError> =
        let hs = (Hierarchy.insert id hierarchy)
        match this.createViewState ctx id with
        | Ok viewState ->
            hierarchy <- hs
            viewStates.Add (id, viewState)
            changeLog.Add(StateChange.ViewAdded(id, viewState.ViewModel))
            viewState.Load()
            Ok (_toList changeLog)
        | Error e -> Error e

    static member Create (hierarchy:Hierarchy, viewModels:Map<HierarchyKey, obj>, viewStates:Map<HierarchyKey, IViewState>, ctx:IForestContext) = 
        (new MutableScope(hierarchy, viewModels, viewStates, ctx)).Init()

    member private this.Init() =
        for kvp in viewStates do 
            kvp.Value.EnterModificationScope this |> ignore
        // TODO: create non-existing view-states, call resume state on the newly created ones
        //
        this

    member internal this.ActivateView(id) =
        ForestOperation.InstantiateView(id) |> this.Update |> ignore
        match viewStates.TryGetValue id with
        | (true, viewState) -> (upcast viewState:IView)
        | (false, _) -> nil<_>

    member internal this.GetOrActivateView (id) : 'TView when 'TView :> IView =
        let result =
            match viewStates.TryGetValue id with
            | (true, viewState) -> (upcast viewState:IView)
            | (false, _) -> this.ActivateView id
        (downcast result:'TView)

    member __.Update (operation: ForestOperation) =
        processChanges ctx operation

    member this.Apply (entry: StateChange) =
        match entry with
        | StateChange.ViewAdded (id, vm) ->
            match HierarchyKey.isShell id with
            | true -> Some (StateError.HierarchyElementAbsent(id))
            | false ->
                let hs = hierarchy |> Hierarchy.insert id
                match viewModels.TryGetValue id with
                | (true, _) -> Some (UnexpectedModelState id)
                | (false, _) ->
                    match this.createViewState ctx id with
                    | Ok viewState ->
                        hierarchy <- hs
                        viewStates.Add (id, viewState)
                        viewState.Resume(vm)
                        None
                    | Error e -> Some e
        | StateChange.ViewDestroyed (id) ->
            match destroyView id with Ok _ -> None | Error e -> Some e
        | StateChange.ViewModelUpdated (id, vm) -> 
            viewStates.[id].Resume(vm)
            None

    member this.Dispose() = 
        for kvp in viewStates do kvp.Value |> this.unvisitViewState
        eventBus.Dispose()

    member __.Deconstruct() = (hierarchy, viewModels |> _toMap, viewStates |> _toMap, changeLog |> _toList)

    member __.Context with get() = ctx

    interface IViewStateModifier with
        member __.GetViewModel id = 
            match viewModels.TryGetValue id with
            | (true, v) -> Some v
            | (false, _) -> None
        member __.SetViewModel silent id vm = 
            viewModels.[id] <- vm
            if not silent then changeLog.Add(StateChange.ViewModelUpdated(id, vm))
            vm
        member this.ActivateView parent region name =
            let id = parent |> HierarchyKey.newKey region name 
            this.ActivateView id
        member this.PublishEvent sender message topics = 
            ForestOperation.PublishEvent(sender.InstanceID,message,topics) |> this.Update |> ignore
        member this.ExecuteCommand issuer command arg =
            ForestOperation.InvokeCommand(issuer.InstanceID, command, arg) |> this.Update |> ignore
        member __.SubscribeEvents view =
            for event in view.Descriptor.Events do
                let handler = Event.Handler(event, view)
                eventBus.Subscribe handler event.Topic |> ignore
        member __.UnsubscribeEvents view =
            eventBus.Unsubscribe view |> ignore
    interface IDisposable with member __.Dispose() = __.Dispose()
