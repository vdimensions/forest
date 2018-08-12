namespace Forest

open Forest
open Forest.Events

open System


type ForestOperation =
    | InstantiateView of RegionID * string
    | UpdateViewModel of Guid * obj
    | HandleEvent of Guid
    | DestroyView of Guid
    | DestroyRegion of Guid
    | InvokeCommand of Guid * string * obj
    | Multiple of ForestOperation list

[<RequireQualifiedAccess>]
module Engine =
    type Error =
        | ViewNotFound of ViewName: string
        | UnexpectedModelState of Path: ViewID
        | MissingModelState of Path: ViewID
        | CommandNotFound of Parameters: ViewID * string
        | CommandBadArgument of Parameters: ViewID * string * Type
        | UnknownOperation of Operation: ForestOperation
        | ExpectedViewKey of Key: Guid
        | CommandError of Cause: Command.Error
        | HierarchyError

    let inline private _toMap (dict: System.Collections.Generic.IDictionary<'a, 'b>) : Map<'a, 'b> =
        dict
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq
    //let private _toMap<'k, 'v> = Seq.map (|KeyValue|) >> Map.ofSeq<'k,'v>
    let inline private _toList (l: System.Collections.Generic.IList<'a>) = List.ofSeq l
    
    type private MutableState(hierarchy: Hierarchy.State, viewModels: Map<Guid, obj>, viewStates: Map<Guid, ViewState>) = 
        let mutable _hierarchy: Hierarchy.State = hierarchy
        let mutable _viewModels: System.Collections.Generic.Dictionary<Guid, obj> = System.Collections.Generic.Dictionary(viewModels)
        let mutable _viewStates: System.Collections.Generic.Dictionary<Guid, ViewState> = System.Collections.Generic.Dictionary(viewStates)

        let mutable _changeLog: System.Collections.Generic.List<State.StateChange>  = System.Collections.Generic.List()

        let _mapHierarchyError (he: Hierarchy.Error) =
            HierarchyError

        let _createViewState(ctx: IForestContext) (evb: IEventBus) (viewID: ViewID) (guid: Guid) =
            let viewName = viewID.Name
            match ctx.ViewRegistry.GetViewMetadata(viewName) with
            | Some vd ->
                let vi = (ctx.ViewRegistry.Resolve vd.Name) :?> IViewInternal
                vi.EventBus <- evb
                vi.InstanceID <- guid
                // TODO: more vi setup

                Ok (ViewState(guid, vd, vi))
            | None -> Error (ViewNotFound viewName)

        let _addViewState (ctx: IForestContext) (evb: IEventBus) (vk: Hierarchy.Key): Result<State.StateChange List, Error> =
            match (Hierarchy.add vk _hierarchy) with
            | Ok (hs, guid) ->
                match (Hierarchy.getViewID guid hs) with
                | Some viewID ->
                    match _createViewState ctx evb viewID guid with
                    | Ok viewState ->
                        _hierarchy <- hs
                        _viewModels.Add (guid, viewState.View.ViewModel)
                        _viewStates.Add (guid, viewState)
                        _changeLog.Add(State.StateChange.ViewAdded(viewID, guid, viewState.View.ViewModel))
                        Ok (_toList _changeLog)
                    | Error e -> Error e
                | None -> (Error (ExpectedViewKey guid))
            | Error he -> Error (_mapHierarchyError he)

        let _updateViewModel (guid: Guid) (vm: obj): Result<State.StateChange List, Error> =
            match (Hierarchy.getViewID guid _hierarchy) with
            | Some viewID ->
                _viewModels.[guid] <- vm
                //_changeLog <- _changeLog @ [(ViewNeedsRefresh viewID)]
                Ok (_toList _changeLog)
            | None -> (Error (ExpectedViewKey guid))

        let _destroyView (guid: Guid): Result<State.StateChange List, Error> =
            match (Hierarchy.getViewID guid _hierarchy) with
            | Some viewID ->
                match Hierarchy.remove (Hierarchy.Key.ViewKey viewID) _hierarchy with
                | Ok (h, guids) ->
                    for removedGuid in guids do
                        _viewModels.Remove removedGuid |> ignore
                        _viewStates.Remove removedGuid |> ignore
                        match Hierarchy.getViewID removedGuid _hierarchy with
                        | Some removedViewID -> _changeLog.Add(State.StateChange.ViewDestroyed(removedViewID, guid))
                        | None -> ()
                    _hierarchy <- h
                    Ok (_toList _changeLog)
                | Error e -> e |> _mapHierarchyError |> Error // TODO map error
            | None -> (Error (ExpectedViewKey guid))

        let _destroyRegion (guid: Guid) : Result<State.StateChange List, Error> =
            match (Hierarchy.getRegionID guid _hierarchy) with
            | Some regionID ->
                match Hierarchy.remove (Hierarchy.Key.RegionKey regionID) _hierarchy with
                | Ok (h, guids) ->
                    for removedGuid in guids do
                        _viewModels.Remove removedGuid |> ignore
                        _viewStates.Remove removedGuid |> ignore
                        match Hierarchy.getViewID removedGuid _hierarchy with
                        | Some removedViewID -> _changeLog.Add(State.StateChange.ViewDestroyed(removedViewID, guid))
                        | None -> ()
                    _hierarchy <- h
                    Ok (_toList _changeLog)
                | Error e -> e |> _mapHierarchyError |> Error // TODO map error
            | None -> (Error (ExpectedViewKey guid))

        let _executeCommand (guid: Guid) (name: string) (arg: obj) : Result<State.StateChange List, Error> =
            // TODO: need to handle a special case with command altering the hierarchy -- adding or deleting a view
            match _viewStates.TryGetValue guid with
            | (true, vs) ->
                match vs.Descriptor.Commands.TryFind name with
                | Some cmd -> 
                    vs.View |> cmd.Invoke arg
                    Ok (_toList _changeLog)
                | None -> 
                    // TODO: change CommandNotFound params
                    Error (CommandNotFound(Unchecked.defaultof<ViewID>, name))
            | (false, _) -> (Error (ExpectedViewKey guid))

        let rec _processChanges (ctx: IForestContext) (eventBus: IEventBus) (operation: ForestOperation) _ =
            match operation with
            | Multiple operations -> _loopStates ctx eventBus operations (_changeLog |> _toList)
            | InstantiateView (region, viewName) -> _addViewState ctx eventBus (Hierarchy.Key.ViewKey (ViewID(region, -1, viewName)))
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

        new() = MutableState(Hierarchy.empty, Map.empty, Map.empty)

        member __.Update (operation: ForestOperation) (ctx: IForestContext) =
            use eventBus = EventBus.Create()
            try
                match _processChanges ctx eventBus operation _changeLog with
                | Ok cl -> cl
                | Error e ->
                    // TODO exception
                    List.empty
            with 
            | _ -> (_toList _changeLog)

        member __.Apply (callResumeState: bool) (ctx: IForestContext) (evb: IEventBus) (entry: State.StateChange) =
            match entry with
            | State.StateChange.ViewAdded (viewID, guid, vm) ->
                match (Hierarchy.insert (Hierarchy.Key.ViewKey viewID) guid _hierarchy) with
                | Ok (hs, guid) ->
                    match _viewModels.TryGetValue guid with
                    | (true, _) -> Some (UnexpectedModelState viewID)
                    | (false, _) ->
                        match _createViewState ctx evb viewID guid with
                        | Ok viewState ->
                            if (callResumeState) then viewState.View.ResumeState(vm)
                            _hierarchy <- hs
                            _viewModels.Add (guid, viewState.View.ViewModel)
                            _viewStates.Add (guid, viewState)
                            None
                        | Error e -> Some e
                | Error he -> Some (_mapHierarchyError he)
            | State.StateChange.ViewDestroyed (viewID, guid) ->
                match Hierarchy.getViewGuid viewID _hierarchy with
                | Ok g ->
                    if (g = guid) 
                    then match _destroyView guid with Ok _ -> None | Error e -> Some e
                    else Some (_mapHierarchyError (Hierarchy.Error.InconsistentGuidMapping(Hierarchy.Key.ViewKey viewID)))
                | Error he -> Some (_mapHierarchyError he)
            | State.StateChange.ViewModelUpdated (viewID, guid, vm) -> 
                None

        member __.Deconstruct() = (_hierarchy, _viewModels |> _toMap, _viewStates |> _toMap)

    type [<Sealed>] private T =
        // transferable across machines
        val mutable private _hierarchy: Hierarchy.State
        // transferable across machines
        val mutable private _viewModels: Map<Guid, obj>

        // non-transferable across machines
        val mutable private _viewStates:  Map<Guid, ViewState>
        // non-transferable across machines
        val mutable _activeMutableState: MutableState option

        val state: Event<State.StateChange List>

        member private this._doApply (sync: bool) (changeLog: State.StateChange List) (ctx: IForestContext) =
            let ms = (new MutableState(this._hierarchy, this._viewModels, this._viewStates))
            let rec _applyChangelog (ct: IForestContext) (eb: IEventBus) (ms: MutableState) (cl: State.StateChange List) =
                match cl with
                | [] -> None
                | [entry] -> entry |> ms.Apply sync ct eb
                | head::tail -> 
                    match _applyChangelog ct eb ms [head] with
                    | Some e -> Some e
                    | None -> _applyChangelog ct eb ms tail
                
            use evb = EventBus.Create()
            match _applyChangelog ctx evb ms changeLog with
            | None ->
                match ms.Deconstruct() with
                | (hs, vm, vs) ->
                    this._hierarchy <- hs
                    this._viewModels <- vm
                    this._viewStates <- vs    
                ()
            | Some error ->
                // TODO: exception
                ()

        member this.Update (operation: ForestOperation) (ctx: IForestContext) =
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
            | Some ms -> ctx |> ms.Update operation |> ignore
            | None ->
                // TODO: invoke steps 1..4
                let ms = (new MutableState(this._hierarchy, this._viewModels, this._viewStates))
                this._activeMutableState <- Some ms
                let c = ctx |> ms.Update operation
                this._activeMutableState <- None
                ctx |> this._doApply false c

        member this.ApplyChangeLog (changeLog: State.StateChange List) (ctx: IForestContext) = ctx |> this._doApply true changeLog