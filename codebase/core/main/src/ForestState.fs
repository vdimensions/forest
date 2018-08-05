namespace Forest

open Forest
open Forest.Events

open System

// ----------------------------------

type ForestOperation =
    | InstantiateView of RegionID * string
    | UpdateViewModel of Guid * obj
    | HandleEvent of Guid
    | DestroyView of Guid
    | DestroyRegion of Guid
    | InvokeCommand of Guid * string * obj
    | Multiple of ForestOperation list

// contains the active mutable forest state, such as the latest dom index and view state changes
type [<Sealed>] internal ViewState(id: Guid, descriptor: IViewDescriptor, viewInstance: IViewInternal) =
    member __.ID with get() = id
    member __.Descriptor with get() = descriptor
    member __.View with get() = viewInstance
   

module State =
    type Error =
        | ViewNotFound of viewName: string
        | UnexpectedModelState of path: ViewID
        | MissingModelState of path: ViewID
        | CommandNotFound of parameters: ViewID * string
        | CommandBadArgument of parameters: ViewID * string * Type
        | UnknownOperation of operation: ForestOperation
        | ExpectedViewKey of key: Guid
        | HierarchyError

    type StateChange =
        | ViewAdded of ViewID
        | ViewNeedsRefresh of ViewID
        | ViewDestroyed of ViewID

    type private StateData = { 
        Hierarchy: Hierarchy.State;
        ViewModels: Map<Guid, obj>;
        ViewStates: Map<Guid, ViewState>;
        ViewInstances: Map<Guid, IViewInternal>;
        ChangeLog: StateChange Set;
    }
    // -----------------------------------------
    let private _mapHierarchyError (he: Hierarchy.Error) =
        HierarchyError

    let private _addViewState (ctx: IForestContext) (evb: IEventBus) (stateData: StateData) (h: Hierarchy.State*Guid): Result<StateData, Error> =
        let (hs, guid) = h
        match (Hierarchy.getViewID guid hs) with
        | Some viewID ->
            let viewName = viewID.Name
            match ctx.ViewRegistry.GetViewMetadata(viewName) with
            | Some vd ->
                let vi = (ctx.ViewRegistry.Resolve viewName) :?> IViewInternal
                vi.EventBus <- evb
                vi.InstanceID <- guid
                // TODO: more vi setup

                let vms = stateData.ViewModels.Add (guid, vi.ViewModel)
                let vss = stateData.ViewStates.Add (guid, ViewState(guid, vd, vi))
                let vis = stateData.ViewInstances.Add (guid, vi)
                let clg = stateData.ChangeLog.Add (ViewAdded viewID)
                Ok ({ Hierarchy = hs; ViewModels = vms; ViewStates = vss; ViewInstances = vis; ChangeLog = clg })
            | None -> Error (ViewNotFound viewName)
        | None -> (Error (ExpectedViewKey guid))

    let private _updateViewModel (guid:Guid) (vm: obj) (sd: StateData): Result<StateData, Error> =
        match (Hierarchy.getViewID guid sd.Hierarchy) with
        | Some viewID ->
            let vm = sd.ViewModels.Remove(guid).Add(guid, vm)
            let cl = sd.ChangeLog.Add (ViewNeedsRefresh viewID)
            Ok { Hierarchy = sd.Hierarchy; ViewModels = vm; ViewStates = sd.ViewStates; ViewInstances = sd.ViewInstances; ChangeLog = cl }
        | None -> (Error (ExpectedViewKey guid))
    // -----------------------------------------

    let rec private _processChanges(ctx: IForestContext) (eventBus: IEventBus) (operation: ForestOperation) (stateData: StateData) =
        match operation with
        | Multiple operations -> _loopStates ctx eventBus operations stateData
        | InstantiateView (region, viewName) ->
            let vk = Hierarchy.Key.ViewKey (ViewID(region, -1, viewName))
            Ok stateData.Hierarchy
            >>>= (Hierarchy.add vk, _mapHierarchyError)
            >>= _addViewState ctx eventBus stateData
        | UpdateViewModel (viewID, vm) -> _updateViewModel viewID vm stateData
        //| DestroyView viewID -> Ok (stateData |> _destroyViews [viewID])
        //| DestroyRegion regionID -> Ok (stateData |> _destroyRegions [regionID])
        //| InvokeCommand (viewID, commandName, arg) -> Ok stateData >>= _executeCommand viewID commandName arg
        | _ -> Error (UnknownOperation operation)
    and private _loopStates c eb ops sd =
        match ops with
        | [] -> Ok sd
        | [op] -> _processChanges c eb op sd
        | head::tail ->
            Ok sd
            >>= _processChanges c eb head
            >>= _loopStates c eb tail

    type [<Sealed>] private T =
        // transferable across machines
        val mutable private _hierarchy: Hierarchy.State
        // transferable across machines
        val mutable private _viewModels: Map<Guid, obj>
        // transferable across machines
        val mutable private _changeLog: StateChange Set

        // non-transferable across machines
        val mutable private _viewStates:  Map<Guid, ViewState>
        // non-transferable across machines
        val mutable private _viewInstances:  Map<Guid, IViewInternal>

        member this.Update (ctx: IForestContext, operation: ForestOperation) =
            let sd = { 
                Hierarchy = this._hierarchy; 
                ViewModels = this._viewModels; 
                ViewStates = this._viewStates; 
                ViewInstances = this._viewInstances; 
                ChangeLog = this._changeLog;
            }
            use eventBus = EventBus.Create()
            match _processChanges ctx eventBus operation sd with
            | Ok state ->
                this._hierarchy <- state.Hierarchy
                this._viewModels <- state.ViewModels
                this._viewStates <- state.ViewStates
                this._viewInstances <- state.ViewInstances
                this._changeLog <- state.ChangeLog
                ()
            | Error e ->
                // TODO exception
                ()
