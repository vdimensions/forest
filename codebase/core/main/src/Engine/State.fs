namespace Forest

open Forest.Events

open System


[<Serializable>]
type [<Struct>] StateError =
    | ViewNotFound of view: string
    | UnexpectedModelState of identifier: Identifier
    | CommandNotFound of owner: Identifier * command: string
    | CommandError of cause: Command.Error
    | HierarchyElementAbsent of orphanIdentifier: Identifier
    | NoViewAdded

[<Serializable>]
type [<Struct>] StateChange =
    | ViewAdded of parent: Identifier * viewModel: obj
    | ViewModelUpdated of id: Identifier * updatedViewModel: obj
    | ViewDestroyed of destroyedViewID: Identifier

[<Serializable>]
type ForestOperation =
    | InstantiateView of parent: Identifier * region: string * viewName: string
    | UpdateViewModel of parent: Identifier * viewModel: obj
    | DestroyView of identifier: Identifier
    | InvokeCommand of owner: Identifier * commandName: string * commandArg: obj
    | Multiple of operations: ForestOperation list

type [<Sealed>] internal MutableScope (hierarchy: Hierarchy, viewModels: Map<Identifier, obj>, viewStates: Map<Identifier, ViewState>, ctx: IForestContext) as self = 
    do
        for kvp in viewStates do 
            kvp.Value |> self._visitViewState |> ignore
        // TODO: create non-existing view-states, call resume state on the newly created ones
        //
    let mutable _hierarchy = hierarchy
    let _eventBus: IEventBus = Forest.Events.Event.create()
    let _viewModels: System.Collections.Generic.Dictionary<Identifier, obj> = System.Collections.Generic.Dictionary(viewModels)
    let _viewStates: System.Collections.Generic.Dictionary<Identifier, ViewState> = System.Collections.Generic.Dictionary(viewStates)

    let _toList (l: System.Collections.Generic.IList<'a>) = List.ofSeq l
    let _toMap (dict: System.Collections.Generic.IDictionary<'a, 'b>) : Map<'a, 'b> =
        dict
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq

    let _changeLog: System.Collections.Generic.List<StateChange>  = System.Collections.Generic.List()

    let _updateViewModel (id: Identifier) (vm: obj): Result<StateChange List, StateError> =
        _viewModels.[id] <- vm
        //_changeLog <- _changeLog @ [(ViewNeedsRefresh viewID)]
        Ok (_toList _changeLog)

    let _destroyView (id: Identifier): Result<StateChange List, StateError> =
        let (h, ids) = Hierarchy.remove id _hierarchy
        for removedID in ids do
            _viewModels.Remove removedID |> ignore
            _viewStates.Remove removedID |> ignore
            _changeLog.Add(StateChange.ViewDestroyed(removedID))
        _hierarchy <- h
        Ok (_toList _changeLog)

    let _executeCommand (id: Identifier) (name: string) (arg: obj) : Result<StateChange List, StateError> =
        match _viewStates.TryGetValue id with
        | (true, vs) ->
            match vs.Descriptor.Commands.TryFind name with
            | Some cmd -> 
                vs.View |> cmd.Invoke arg
                Ok (_toList _changeLog)
            | None ->  Error (CommandNotFound(id, name))
        | (false, _) -> (Error (ViewNotFound id.View))

    let rec _processChanges (ctx: IForestContext) (operation: ForestOperation) _ =
        match operation with
        | Multiple operations -> _loopStates ctx operations (_changeLog |> _toList)
        | InstantiateView (parent, region, viewName) -> self._addViewState ctx parent region viewName
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

    member private __._createViewState(ctx: IForestContext) (id: Identifier) =
        match ctx.ViewRegistry.GetDescriptor(id.View) |> null2vopt with
        | ValueSome vd ->
            let vi = (ctx.ViewRegistry.Resolve id.View) :?> IViewState
            vi.InstanceID <- id
            ViewState(id, vd, vi) |> (self._visitViewState >> Ok) // will also set the view model
        | ValueNone -> Error (ViewNotFound id.View)

    member private __._visitViewState (vs:ViewState) =
        vs.View.EventBus <- _eventBus
        vs.View.Descriptor <- vs.Descriptor
        // NB: this setter triggers internal logic, it must be called last
        vs.View.EnterModificationScope self
        vs

    member private __._unvisitViewState (vs:ViewState) =
        // NB: this setter triggers internal logic, it must be called first
        vs.View.LeaveModificationScope self
        vs.View.EventBus <- nil<IEventBus>
        ()

    member private __._addViewState (ctx: IForestContext) (parent: Identifier) (region: string) (viewName: string): Result<StateChange List, StateError> =
        let (hs, id) = (Hierarchy.add parent region viewName _hierarchy)
        match self._createViewState ctx id with
        | Ok viewState ->
            _hierarchy <- hs
            _viewStates.Add (id, viewState)
            _changeLog.Add(StateChange.ViewAdded(id, viewState.View.ViewModel))
            viewState.View.Load()
            Ok (_toList _changeLog)
        | Error e -> Error e

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
        | StateChange.ViewAdded (viewID, vm) ->
            match Identifier.isShell viewID with
            | true -> Some (StateError.HierarchyElementAbsent(viewID))
            | false ->
                let (hs, id) = _hierarchy |> Hierarchy.insert viewID.Parent viewID.UniqueID viewID.Region viewID.View
                match _viewModels.TryGetValue id with
                | (true, _) -> Some (UnexpectedModelState viewID)
                | (false, _) ->
                    match self._createViewState ctx id with
                    | Ok viewState ->
                        _hierarchy <- hs
                        _viewStates.Add (id, viewState)
                        _viewModels.[id] <- viewState.View.ViewModel
                        //if (callResumeState) then viewState.View.ResumeState()
                        viewState.View.Load()
                        None
                    | Error e -> Some e
        | StateChange.ViewDestroyed (viewID) ->
            match _destroyView viewID with Ok _ -> None | Error e -> Some e
        | StateChange.ViewModelUpdated (viewID, vm) -> 
            _viewModels.[viewID] <- vm
            None

    member __.Dispose() = 
        for kvp in _viewStates do kvp.Value |> self._unvisitViewState
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
        member __.ActivateView parent region name =
            let inline vsd changeList =                    
                match changeList with
                | [] -> Error <| NoViewAdded
                | list -> list |> (List.last >> Ok)

            let inline getViewState (vs: System.Collections.Generic.Dictionary<Identifier, ViewState>) ch =
                match ch with
                | StateChange.ViewAdded (id, _) -> 
                    match vs.TryGetValue id with
                    | (true, viewState) -> Ok (upcast viewState.View : IView)
                    | (false, _) -> Error (ViewNotFound (id.View) )
                | _ -> Error NoViewAdded
            match ForestOperation.InstantiateView(parent, region, name) |> ((self.Update >> vsd) >>= (getViewState _viewStates)) with
            | Ok view -> view
            | Error e -> failwith (e.ToString())
        member __.SubscribeEvents view =
            for event in view.Descriptor.Events do
                let handler = Event.Handler(event, view)
                for topic in event.Topics do _eventBus.Subscribe handler topic |> ignore
        member __.UnsubscribeEvents view =
            _eventBus.Unsubscribe view |> ignore
    interface IDisposable with member __.Dispose() = self.Dispose()

//type State1 =
//    | Empty
//    | Immutable of Hierarchy * Map<Identifier, obj> * Map<Identifier, ViewState>
//    | Mutable of MutableScope

[<Serializable>]
type State internal(hierarchy: Hierarchy, viewModels: Map<Identifier, obj>, viewStates:  Map<Identifier, ViewState>, fid: ForestID) =
    internal new (hierarchy: Hierarchy, viewModels: Map<Identifier, obj>, viewStates:  Map<Identifier, ViewState>) = State(hierarchy, viewModels, viewStates, ForestID.newID())
    [<CompiledName("Empty")>]
    static member empty = State(Hierarchy.empty, Map.empty, Map.empty, ForestID.empty)
    member internal __.Hierarchy with get() = hierarchy
    member internal __.ViewModels with get() = viewModels
    member internal __.ViewStates with get() = viewStates
    member __.Hash with get() = fid.Hash
    member __.Host with get() = fid.Host

type [<Interface>] IStateVisitor =
    abstract member BFS: id:Identifier -> region:string -> view:string -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    abstract member DFS: id:Identifier -> region:string -> view:string -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit

[<RequireQualifiedAccess>]
module internal State =
    let create (hs, vm, vs) = State(hs, vm, vs)

    let discardViewStates (st: State) = State(st.Hierarchy, st.ViewModels, Map.empty)

    let rec private _traverseState (v: IStateVisitor) parent (ids: Identifier list) (st: State) =
        match ids with
        | [] -> ()
        | head::tail ->
            let ix = 0 // TODO
            let vm = st.ViewModels.[head]
            let descriptor = st.ViewStates.[head].View.Descriptor
            v.BFS parent head.Region head.View ix vm descriptor
            // visit siblings 
            _traverseState v parent tail st
            // visit children
            _traverseState v head st.Hierarchy.Hierarchy.[head] st
            v.DFS parent head.Region head.View ix vm descriptor
            ()

    let traverse (v: IStateVisitor) (st: State) =
        let root = Identifier.shell
        let ch = st.Hierarchy.Hierarchy.[root]
        _traverseState v root ch st
