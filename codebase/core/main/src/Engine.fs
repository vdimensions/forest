namespace Forest

open Forest
open Forest.Events

open System


type ForestOperation =
    | InstantiateView of Identifier * string
    | UpdateViewModel of Identifier * obj
    | HandleEvent of Identifier
    | DestroyView of Identifier
    | DestroyRegion of Identifier
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
        | ExpectedViewKey of Key: Identifier
        | CommandError of Cause: Command.Error
        | HierarchyElementAbsent of id: Identifier

    let inline private _toMap (dict: System.Collections.Generic.IDictionary<'a, 'b>) : Map<'a, 'b> =
        dict
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq
    //let private _toMap<'k, 'v> = Seq.map (|KeyValue|) >> Map.ofSeq<'k,'v>
    let inline private _toList (l: System.Collections.Generic.IList<'a>) = List.ofSeq l
    
    type private MutableState private (hierarchy: Hierarchy.State, viewModels: Map<Identifier, obj>, viewStates: Map<Identifier, ViewState>, eventBus: IEventBus) as self = 
        do for kvp in viewStates do kvp.Value |> self._visitViewState eventBus |> ignore
        let mutable _hierarchy = hierarchy
        let _viewModels: System.Collections.Generic.Dictionary<Identifier, obj> = System.Collections.Generic.Dictionary(viewModels)
        let _viewStates: System.Collections.Generic.Dictionary<Identifier, ViewState> = System.Collections.Generic.Dictionary(viewStates)

        let _changeLog: System.Collections.Generic.List<State.StateChange>  = System.Collections.Generic.List()

        let _updateViewModel (id: Identifier) (vm: obj): Result<State.StateChange List, Error> =
            match (Identifier.isView id) with
            | true ->
                _viewModels.[id] <- vm
                //_changeLog <- _changeLog @ [(ViewNeedsRefresh viewID)]
                Ok (_toList _changeLog)
            | false -> (Error (ExpectedViewKey id))

        let _destroyView (id: Identifier): Result<State.StateChange List, Error> =
            match (Identifier.isView id) with
            | true ->
                let (h, ids) = Hierarchy.remove id _hierarchy
                for removedID in ids do
                    _viewModels.Remove removedID |> ignore
                    _viewStates.Remove removedID |> ignore
                    if Identifier.isView removedID then _changeLog.Add(State.StateChange.ViewDestroyed(removedID))
                _hierarchy <- h
                Ok (_toList _changeLog)
            | false -> (Error (ExpectedViewKey id))

        let _destroyRegion (id: Identifier) : Result<State.StateChange List, Error> =
            match Identifier.isRegion id with
            | true ->
                let (h, ids) = Hierarchy.remove id _hierarchy
                for removedID in ids do
                    _viewModels.Remove removedID |> ignore
                    _viewStates.Remove removedID |> ignore
                    if Identifier.isView removedID then _changeLog.Add(State.StateChange.ViewDestroyed(removedID))
                _hierarchy <- h
                Ok (_toList _changeLog)
            | false -> (Error (ExpectedViewKey id))

        let _executeCommand (id: Identifier) (name: string) (arg: obj) : Result<State.StateChange List, Error> =
            match _viewStates.TryGetValue id with
            | (true, vs) ->
                match vs.Descriptor.Commands.TryFind name with
                | Some cmd -> 
                    vs.View |> cmd.Invoke arg
                    Ok (_toList _changeLog)
                | None ->  Error (CommandNotFound(id, name))
            | (false, _) -> (Error (ExpectedViewKey id))

        let rec _processChanges (ctx: IForestContext) (eventBus: IEventBus) (operation: ForestOperation) _ =
            match operation with
            | Multiple operations -> _loopStates ctx eventBus operations (_changeLog |> _toList)
            | InstantiateView (region, viewName) -> self._addViewState ctx eventBus region viewName
            | UpdateViewModel (viewID, vm) -> _updateViewModel viewID vm
            | DestroyView viewID -> _destroyView viewID
            | DestroyRegion regionID -> _destroyRegion regionID
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
            let viewName = Identifier.nameof id
            match ctx.ViewRegistry.GetViewMetadata(viewName) |> null2opt with
            | Some vd ->
                let vi = (ctx.ViewRegistry.Resolve vd.Name) :?> IViewInternal
                vi.InstanceID <- id
                ViewState(id, vd, vi) |> (self._visitViewState evb >> Ok)
            | None -> Error (ViewNotFound viewName)

        new(state: State) = MutableState(state.Hierarchy, state.ViewModels, state.ViewStates, EventBus.create())

        member private __._visitViewState (evb: IEventBus) (vs:ViewState) =
            // TODO: more vi setup
            vs.View.EventBus <- evb
            vs.View.ViewModelProvider <- (upcast self : IViewModelProvider)
            vs

        member private __._unvisitViewState (vs:ViewState) =
            // TODO: more vi setup
            vs.View.ViewModelProvider <- Unchecked.defaultof<IViewModelProvider>
            vs.View.EventBus <- Unchecked.defaultof<IEventBus>
            ()

        member private __._addViewState (ctx: IForestContext) (evb: IEventBus) (region: Identifier) (viewName: string): Result<State.StateChange List, Error> =
            let (hs, id) = (Hierarchy.add region viewName _hierarchy)
            match (Identifier.isView id) with
            | true ->
                match _createViewState ctx evb id with
                | Ok viewState ->
                    _hierarchy <- hs
                    _viewModels.Add (id, viewState.View.ViewModel)
                    _viewStates.Add (id, viewState)
                    _changeLog.Add(State.StateChange.ViewAdded(id, viewState.View.ViewModel))
                    Ok (_toList _changeLog)
                | Error e -> Error e
            | false -> (Error (ExpectedViewKey id))

        member __.Update (operation: ForestOperation) (ctx: IForestContext) =
            try
                match _processChanges ctx eventBus operation _changeLog with
                | Ok cl -> cl
                | Error e ->
                    // TODO exception
                    List.empty
            with 
            | _ -> (_toList _changeLog)

        member this.Apply (callResumeState: bool) (ctx: IForestContext) (entry: State.StateChange) =
            match entry with
            | State.StateChange.ViewAdded (viewID, vm) ->
                match Identifier.parentOf viewID with
                | Some parent ->
                    match Identifier.view viewID with
                    | Some (name, guid) ->
                        let (hs, id) = (Hierarchy.insert parent guid name _hierarchy)
                        match _viewModels.TryGetValue id with
                        | (true, _) -> Some (UnexpectedModelState viewID)
                        | (false, _) ->
                            match _createViewState ctx eventBus id with
                            | Ok viewState ->
                                if (callResumeState) then viewState.View.ResumeState(vm)
                                _hierarchy <- hs
                                _viewModels.Add (id, viewState.View.ViewModel)
                                _viewStates.Add (id, viewState)
                                None
                            | Error e -> Some e
                    | None -> Some (Error.ExpectedViewKey(viewID))
                | None -> Some (Error.HierarchyElementAbsent(viewID))
            | State.StateChange.ViewDestroyed (viewID) ->
                match Identifier.isView viewID with
                | true -> match _destroyView viewID with Ok _ -> None | Error e -> Some e
                | false -> Some (Error.ExpectedViewKey(viewID))
            | State.StateChange.ViewModelUpdated (viewID, vm) -> 
                // TODO
                None

        member __.Dispose() = 
            for kvp in _viewStates do kvp.Value |> self._unvisitViewState
            eventBus.Dispose()

        member __.Deconstruct() = (_hierarchy, _viewModels |> _toMap, _viewStates |> _toMap)

        interface IViewModelProvider with
             member __.GetViewModel id = _viewModels.[id]
             member __.SetViewModel id vm = _viewModels.[id] <- vm

        interface IDisposable with member __.Dispose() = self.Dispose()

    let private _applyStateChanges(sync: bool) (changeLog: State.StateChange List) (ctx: IForestContext) (state: State) =
        use ms = new MutableState(state)
        let rec _applyChangelog (ct: IForestContext) (ms: MutableState) (cl: State.StateChange List) =
            match cl with
            | [] -> None
            | [entry] -> entry |> ms.Apply sync ct
            | head::tail -> 
                match _applyChangelog ct ms [head] with
                | Some e -> Some e
                | None -> _applyChangelog ct ms tail
                
        
        match _applyChangelog ctx ms changeLog with
        | None ->
            match ms.Deconstruct() with
            | data -> 
                // TODO: create guid
                State.create data
        | Some error ->
            // TODO: exception
            state

    type [<Sealed>] private T =
        val mutable _activeMutableState: MutableState option

        val stateChanged: Event<State.StateChange List>

        member this.Update (operation: ForestOperation) (ctx: IForestContext) (state: State) =
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
            match this._activeMutableState with
            | Some ms -> 
                ms.Update operation ctx |> ignore
                // TODO: preserve guid
                State.create (ms.Deconstruct())
            | None ->
                // TODO: invoke steps 1..4
                let ms = new MutableState(state)
                let c = 
                    try
                        this._activeMutableState <- Some ms
                        ctx |> ms.Update operation
                    finally
                        this._activeMutableState <- None
                        ms.Dispose()
                _applyStateChanges false c ctx state

        member __.ApplyChangeLog (changeLog: State.StateChange List) (ctx: IForestContext) (state: State) = 
            _applyStateChanges true changeLog ctx state