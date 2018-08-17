namespace Forest

open Forest
open Forest.Events

open System


type ForestOperation =
    | InstantiateView of Identifier * string * string
    | UpdateViewModel of Identifier * obj
    | HandleEvent of Identifier
    | DestroyView of Identifier
    | InvokeCommand of Identifier * string * obj
    | Multiple of ForestOperation list

[<RequireQualifiedAccess>]
module Engine =
    type Error =
        | ViewNotFound of ViewName: string
        | UnexpectedModelState of Path: Identifier
        | MissingModelState of Path: Identifier
        | CommandNotFound of Parameters: Identifier * string
        | CommandBadArgument of Parameters: Identifier * string * Type
        | UnknownOperation of Operation: ForestOperation
        | CommandError of Cause: Command.Error
        | HierarchyElementAbsent of id: Identifier
        | NoViewAdded

    let inline private _toMap (dict: System.Collections.Generic.IDictionary<'a, 'b>) : Map<'a, 'b> =
        dict
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq
    //let private _toMap<'k, 'v> = Seq.map (|KeyValue|) >> Map.ofSeq<'k,'v>
    let inline private _toList (l: System.Collections.Generic.IList<'a>) = List.ofSeq l
    
    type private MutableState private (hierarchy: Hierarchy, viewModels: Map<Identifier, obj>, viewStates: Map<Identifier, ViewState>, eventBus: IEventBus, ctx: IForestContext) as self = 
        do for kvp in viewStates do kvp.Value |> self._visitViewState eventBus |> ignore
        let mutable _hierarchy = hierarchy
        let _viewModels: System.Collections.Generic.Dictionary<Identifier, obj> = System.Collections.Generic.Dictionary(viewModels)
        let _viewStates: System.Collections.Generic.Dictionary<Identifier, ViewState> = System.Collections.Generic.Dictionary(viewStates)

        let _changeLog: System.Collections.Generic.List<State.StateChange>  = System.Collections.Generic.List()

        let _updateViewModel (id: Identifier) (vm: obj): Result<State.StateChange List, Error> =
            _viewModels.[id] <- vm
            //_changeLog <- _changeLog @ [(ViewNeedsRefresh viewID)]
            Ok (_toList _changeLog)

        let _destroyView (id: Identifier): Result<State.StateChange List, Error> =
            let (h, ids) = Hierarchy.remove id _hierarchy
            for removedID in ids do
                _viewModels.Remove removedID |> ignore
                _viewStates.Remove removedID |> ignore
                _changeLog.Add(State.StateChange.ViewDestroyed(removedID))
            _hierarchy <- h
            Ok (_toList _changeLog)

        let _executeCommand (id: Identifier) (name: string) (arg: obj) : Result<State.StateChange List, Error> =
            match _viewStates.TryGetValue id with
            | (true, vs) ->
                match vs.Descriptor.Commands.TryFind name with
                | Some cmd -> 
                    vs.View |> cmd.Invoke arg
                    Ok (_toList _changeLog)
                | None ->  Error (CommandNotFound(id, name))
            | (false, _) -> (Error (ViewNotFound id.Name))

        let rec _processChanges (ctx: IForestContext) (eventBus: IEventBus) (operation: ForestOperation) _ =
            match operation with
            | Multiple operations -> _loopStates ctx eventBus operations (_changeLog |> _toList)
            | InstantiateView (parent, region, viewName) -> self._addViewState ctx eventBus parent region viewName
            | UpdateViewModel (viewID, vm) -> _updateViewModel viewID vm
            | DestroyView viewID -> _destroyView viewID
            | InvokeCommand (viewID, commandName, arg) -> _executeCommand viewID commandName arg
            | _ -> Error (UnknownOperation operation)
        and _loopStates ctx eventBus ops cl =
            match ops with
            | [] -> Ok cl
            | [op] -> _processChanges ctx eventBus op cl
            | head::tail ->
                Ok List.empty 
                |>= _processChanges ctx eventBus head
                |>= _loopStates ctx eventBus tail     

        let _createViewState(ctx: IForestContext) (evb: IEventBus) (id: Identifier) =
            match ctx.ViewRegistry.GetViewMetadata(id.Name) |> null2opt with
            | Some vd ->
                let vi = (ctx.ViewRegistry.Resolve id.Name) :?> IViewInternal
                vi.InstanceID <- id
                ViewState(id, vd, vi) |> (self._visitViewState evb >> Ok)
            | None -> Error (ViewNotFound id.Name)

        new(state: State, ctx: IForestContext) = MutableState(state.Hierarchy, state.ViewModels, state.ViewStates, EventBus.create(), ctx)

        member private __._visitViewState (evb: IEventBus) (vs:ViewState) =
            // TODO: more vi setup
            vs.View.EventBus <- evb
            vs.View.ViewStateModifier <- (upcast self : IViewStateModifier)
            vs

        member private __._unvisitViewState (vs:ViewState) =
            // TODO: more vi setup
            vs.View.ViewStateModifier <- nil<IViewStateModifier>
            vs.View.EventBus <- nil<IEventBus>
            ()

        member private __._addViewState (ctx: IForestContext) (evb: IEventBus) (parent: Identifier) (region: string) (viewName: string): Result<State.StateChange List, Error> =
            let (hs, id) = (Hierarchy.add parent region viewName _hierarchy)
            match _createViewState ctx evb id with
            | Ok viewState ->
                _hierarchy <- hs
                _viewStates.Add (id, viewState)
                _changeLog.Add(State.StateChange.ViewAdded(id, viewState.View.ViewModel))
                Ok (_toList _changeLog)
            | Error e -> Error e

        member __.Update (operation: ForestOperation) =
            try
                match _processChanges ctx eventBus operation _changeLog with
                | Ok cl -> cl
                | Error e ->
                    // TODO exception
                    List.empty
            with 
            | _ -> (_toList _changeLog)

        member this.Apply (callResumeState: bool) (entry: State.StateChange) =
            match entry with
            | State.StateChange.ViewAdded (viewID, vm) ->
                match Identifier.isShell viewID with
                | true -> Some (Error.HierarchyElementAbsent(viewID))
                | false ->
                    let (hs, id) = _hierarchy |> Hierarchy.insert viewID.Parent viewID.UniqueID viewID.Region viewID.Name
                    match _viewModels.TryGetValue id with
                    | (true, _) -> Some (UnexpectedModelState viewID)
                    | (false, _) ->
                        match _createViewState ctx eventBus id with
                        | Ok viewState ->
                            if (callResumeState) then viewState.View.ResumeState(vm)
                            _hierarchy <- hs
                            _viewStates.Add (id, viewState)
                            _viewModels.[id] = viewState.View.ViewModel
                            None
                        | Error e -> Some e
            | State.StateChange.ViewDestroyed (viewID) ->
                 match _destroyView viewID with Ok _ -> None | Error e -> Some e
            | State.StateChange.ViewModelUpdated (viewID, vm) -> 
                _viewModels.[viewID] <- vm
                None

        member __.Dispose() = 
            for kvp in _viewStates do kvp.Value |> self._unvisitViewState
            eventBus.Dispose()

        member __.Deconstruct() = (_hierarchy, _viewModels |> _toMap, _viewStates |> _toMap)

        interface IViewStateModifier with
             member __.GetViewModel id = 
                match _viewModels.TryGetValue id with
                | (true, v) -> Some v
                | (false, _) -> None
             member __.SetViewModel silent id vm = 
                _viewModels.[id] <- vm
                if not silent then _changeLog.Add(State.StateChange.ViewModelUpdated(id, vm))
             member __.ActivateView parent region name =
                let inline vsd changeList =
                    match changeList with
                    | head::_ -> Ok head
                    | _ -> Error <| NoViewAdded

                let inline getViewState (vs: System.Collections.Generic.Dictionary<Identifier, ViewState>) ch =
                    match ch with
                    | State.StateChange.ViewAdded (id, _) -> 
                        match vs.TryGetValue id with
                        | (true, viewState) -> Ok (upcast viewState.View : IView)
                        | (false, _) -> Error (ViewNotFound (id.Name) )
                    | _ -> Error NoViewAdded

                match ForestOperation.InstantiateView(parent, region, name) |> ((self.Update >> vsd) >>= (getViewState _viewStates)) with
                | Ok view -> view
                | Error e -> failwith (e.ToString())

        interface IDisposable with member __.Dispose() = self.Dispose()

    let inline private _applyStateChanges (mutableState:MutableState) (sync: bool) (changeLog: State.StateChange List) (ctx: IForestContext) (state: State) =
        let rec _applyChangelog (ms: MutableState) (cl: State.StateChange List) =
            match cl with
            | [] -> None
            | head::tail -> 
                match head |> ms.Apply sync with
                | Some e -> Some e
                | None -> _applyChangelog ms tail
                
        
        match _applyChangelog mutableState changeLog with
        | None ->
            match mutableState.Deconstruct() with
            | data -> 
                // TODO: create state guid
                State.create data
        | Some error ->
            //always discard the viewstates upon error
            state |> State.discardViewStates
            // TODO: exception or error view stuff


    let ApplyChangeLog (ctx: IForestContext) (state: State) (changeLog: State.StateChange List) = 
        use ms = new MutableState(state, ctx)
        _applyStateChanges ms true changeLog ctx state

    let Update (ctx: IForestContext) (state: State) (operation: ForestOperation) =
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
        use ms = new MutableState(state, ctx)
        let c = ms.Update operation
        //_applyStateChanges ms false c ctx state
        // TODO: raise event for state changes
        State.create <| ms.Deconstruct()
