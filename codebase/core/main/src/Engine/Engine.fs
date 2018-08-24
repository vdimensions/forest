namespace Forest

open Forest
open Forest.Events

open System

[<Serializable>]
type ForestOperation =
    | InstantiateView of id: HierarchyKey
    | UpdateViewModel of parent: HierarchyKey * viewModel: obj
    | DestroyView of identifier: HierarchyKey
    | InvokeCommand of owner: HierarchyKey * commandName: string * commandArg: obj
    | Multiple of operations: ForestOperation list

type [<Sealed>] internal MutableScope private (hierarchy: Hierarchy, viewModels: Map<HierarchyKey, obj>, viewStates: Map<HierarchyKey, IViewState>, ctx: IForestContext) as self = 
    let mutable _hierarchy = hierarchy
    let _eventBus:IEventBus = Event.createEventBus()
    let _viewModels:System.Collections.Generic.Dictionary<HierarchyKey, obj> = System.Collections.Generic.Dictionary(viewModels)
    let _viewStates:System.Collections.Generic.Dictionary<HierarchyKey, IViewState> = System.Collections.Generic.Dictionary(viewStates)

    let _toList (l: System.Collections.Generic.IList<'a>) = List.ofSeq l
    let _toMap (dict: System.Collections.Generic.IDictionary<'a, 'b>) : Map<'a, 'b> =
        dict
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq

    let _changeLog: System.Collections.Generic.List<StateChange>  = System.Collections.Generic.List()

    let _updateViewModel (id: HierarchyKey) (vm: obj): Result<StateChange List, StateError> =
        _viewModels.[id] <- vm
        //_changeLog <- _changeLog @ [(ViewNeedsRefresh viewID)]
        Ok (_toList _changeLog)

    let _destroyView (id: HierarchyKey): Result<StateChange List, StateError> =
        let (h, ids) = Hierarchy.remove id _hierarchy
        for removedID in ids do
            _viewModels.Remove removedID |> ignore
            _viewStates.Remove removedID |> ignore
            _changeLog.Add(StateChange.ViewDestroyed(removedID))
        _hierarchy <- h
        Ok (_toList _changeLog)

    let _executeCommand (id: HierarchyKey) (name: string) (arg: obj) : Result<StateChange List, StateError> =
        match _viewStates.TryGetValue id with
        | (true, vs) ->
            match vs.Descriptor.Commands.TryFind name with
            | Some cmd -> 
                vs |> cmd.Invoke arg
                Ok (_toList _changeLog)
            | None ->  Error (CommandNotFound(id, name))
        | (false, _) -> (Error (ViewNotFound id.View))

    let rec _processChanges (ctx: IForestContext) (operation: ForestOperation) _ =
        match operation with
        | Multiple operations -> _loopStates ctx operations (_changeLog |> _toList)
        | InstantiateView (id) -> self._addViewState ctx id
        | UpdateViewModel (viewID, vm) -> _updateViewModel viewID vm
        | DestroyView viewID -> _destroyView viewID
        | InvokeCommand (viewID, commandName, arg) -> _executeCommand viewID commandName arg
        //| _ -> Error (UnknownOperation operation)
    and _loopStates ctx ops cl =
        match ops with
        | [] -> Ok cl
        | [op] -> _processChanges ctx op cl
        | head::tail ->
            Ok List.empty 
            |>= _processChanges ctx head
            |>= _loopStates ctx tail

    member private __._createViewState(ctx: IForestContext) (id: HierarchyKey) =
        match ctx.ViewRegistry.GetDescriptor(id.View) |> null2vopt with
        | ValueSome vd ->
            let vi = (ctx.ViewRegistry.Resolve id.View) :?> IViewState
            vi.InstanceID <- id
            vi.Descriptor <- vd
            vi.EnterModificationScope __
            Ok vi // will also set the view model
        | ValueNone -> Error (ViewNotFound id.View)

    member private __._unvisitViewState (vs:IViewState) =
        // NB: this setter triggers internal logic, it must be called first
        vs.LeaveModificationScope __
        ()

    member private __._addViewState (ctx: IForestContext) (id: HierarchyKey): Result<StateChange List, StateError> =
        let hs = (Hierarchy.insert id _hierarchy)
        match __._createViewState ctx id with
        | Ok viewState ->
            _hierarchy <- hs
            _viewStates.Add (id, viewState)
            _changeLog.Add(StateChange.ViewAdded(id, viewState.ViewModel))
            viewState.Load()
            Ok (_toList _changeLog)
        | Error e -> Error e

    static member Create (hierarchy: Hierarchy, viewModels: Map<HierarchyKey, obj>, viewStates: Map<HierarchyKey, IViewState>, ctx: IForestContext) = 
        (new MutableScope(hierarchy, viewModels, viewStates, ctx)).Init()

    member this.Init() =
        for kvp in viewStates do 
            kvp.Value.EnterModificationScope this |> ignore
        // TODO: create non-existing view-states, call resume state on the newly created ones
        //
        this

    member __.Update (operation: ForestOperation) =
        try
            match _processChanges ctx operation _changeLog with
            | Ok cl -> cl
            | Error e ->
                // TODO exception
                List.empty
        with 
        | _ -> (_toList _changeLog)

    member __.Apply (callResumeState: bool) (entry: StateChange) =
        match entry with
        | StateChange.ViewAdded (id, vm) ->
            match HierarchyKey.isShell id with
            | true -> Some (StateError.HierarchyElementAbsent(id))
            | false ->
                let hs = _hierarchy |> Hierarchy.insert id
                match _viewModels.TryGetValue id with
                | (true, _) -> Some (UnexpectedModelState id)
                | (false, _) ->
                    match __._createViewState ctx id with
                    | Ok viewState ->
                        _hierarchy <- hs
                        _viewStates.Add (id, viewState)
                        _viewModels.[id] <- vm//viewState.ViewModel
                        //if (callResumeState) then viewState.View.ResumeState()
                        viewState.Resume(vm)
                        None
                    | Error e -> Some e
        | StateChange.ViewDestroyed (viewID) ->
            match _destroyView viewID with Ok _ -> None | Error e -> Some e
        | StateChange.ViewModelUpdated (viewID, vm) -> 
            _viewModels.[viewID] <- vm
            _viewStates.[viewID].Resume(vm)
            None

    member __.Dispose() = 
        for kvp in _viewStates do kvp.Value |> __._unvisitViewState
        _eventBus.Dispose()

    member __.Deconstruct() = (_hierarchy, _viewModels |> _toMap, _viewStates |> _toMap)

    interface IViewStateModifier with
        member __.GetViewModel id = 
            match _viewModels.TryGetValue id with
            | (true, v) -> Some v
            | (false, _) -> None
        member __.SetViewModel silent id vm = 
            _viewModels.[id] <- vm
            if not silent then _changeLog.Add(StateChange.ViewModelUpdated(id, vm))
            vm
        member this.ActivateView parent region name =
            let id = parent |> HierarchyKey.newKey region name 
            ForestOperation.InstantiateView(id) |> this.Update |> ignore
            match _viewStates.TryGetValue id with
            | (true, viewState) -> (upcast viewState : IView)
            | (false, _) -> nil<_>
        member __.PublishEvent sender message topics = 
            // pass trough Update
            _eventBus.Publish(sender, message, topics)
        member this.ExecuteCommand issuer command arg =
            ForestOperation.InvokeCommand(issuer.InstanceID, command, arg) |> this.Update |> ignore
        member __.SubscribeEvents view =
            for event in view.Descriptor.Events do
                let handler = Event.Handler(event, view)
                _eventBus.Subscribe handler event.Topic |> ignore
        member __.UnsubscribeEvents view =
            _eventBus.Unsubscribe view |> ignore
    interface IDisposable with member __.Dispose() = __.Dispose()

type [<Sealed>] EngineResult internal (state:State, changeList:ChangeList) = 
    do
        ignore <| isNotNull "state" state
        ignore <| isNotNull "changeList" changeList
    member __.State with get() = state
    member __.ChangeList with get() = changeList


[<RequireQualifiedAccess>]
module Engine =
    let inline private _applyStateChanges (mutableState:MutableScope) (sync: bool) (changeList:ChangeList) (ctx: IForestContext) (state: State) =
        let rec _applyChangelog (ms: MutableScope) (cl: StateChange List) =
            match cl with
            | [] -> None
            | head::tail -> 
                match head |> ms.Apply sync with
                | Some e -> Some e
                | None -> _applyChangelog ms tail
                
        
        match _applyChangelog mutableState (changeList.ToList()) with
        | None ->
            match mutableState.Deconstruct() with
            | (a, b, c) -> 
                // TODO: create state guid
                State.createWithFuid (a, b, c, changeList.Fuid)
        | Some error ->
            //always discard the viewstates upon error
            state |> State.discardViewStates
            // TODO: exception or error view stuff


    let ApplyChangeLog (ctx: IForestContext) (state: State) (changeList: ChangeList) = 
        use ms = MutableScope.Create(state.Hierarchy, state.ViewModels, state.ViewStates, ctx)
        let newState = _applyStateChanges ms true changeList ctx state
        EngineResult(newState, changeList)

    let Update (ctx: IForestContext) (operation: ForestOperation) (state: State) =
        // when forest engine kicks in then this is what must happen:
        // 1 - the engine initially keeps an immutable state of the current views and viewmodels
        // 2 - the engine consults the hierarchy and creates a mutable state instance
        // 3 - during step 2 the engine will instantiate any missing view instances (if sync-ed from another machine)
        // 4 - if step 3 yields a collection of re-instantiated views, their respective resume method is called
        // 5 - the engine proceeds with executing the necessary commands or hierarchy changes
        // 6 - during step 5 the engine records a special change log collection
        // 7 - when the processing finishes with success, the engine uses the changelog from step 6 so that
        //     the changes are translated to the immutable state
        // 8 - when step 7 completes, the engine raises an event with the changelog - 
        //     this is the hooking point for replicating the changelog on another machine

        // TODO: invoke steps 1..4
        use ms = MutableScope.Create(state.Hierarchy, state.ViewModels, state.ViewStates, ctx)
        let c = ms.Update operation
        //_applyStateChanges ms false c ctx state
        // TODO: raise event for state changes
        let newState = State.create <| ms.Deconstruct()
        EngineResult(newState, ChangeList(state.Hash, c, newState.Fuid))
