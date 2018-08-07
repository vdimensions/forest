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
        | CommandError of innerError: Command.Error

    type StateChange =
        | ViewAdded of ViewID
        | ViewNeedsRefresh of ViewID
        | ViewDestroyed of ViewID

    type private StateData = { 
        Hierarchy: Hierarchy.State;
        ViewModels: Map<Guid, obj>;
        ViewStates: Map<Guid, ViewState>;
        ChangeLog: StateChange Set;
    }
    // -----------------------------------------
    type private MutableState = 
        let mutable _hierarchy: Hierarchy.State
        let mutable _viewModels: Map<Guid, obj>
        let mutable _viewStates: Map<Guid, ViewState>
        let mutable _changeLog: StateChange Set

        // when forest engine kicks in then this is what must happen:
        // 1 - the engine initially keeps an immutable state of the current views and viewmodels
        // 2 - the engine consults the hierarchy and creates a mutable state instance
        // 3 - during step 2 the engine will instantiate any missing view instances (if sync-ed from another machine)
        // 4 - if step 3 yields a collection of re-instantiated views, their respective resume method is called
        // 5 - the engine proceeds with executing the necessary commands or hierarchy changes
        // 6 - during step 5 the engine records a special change log collection
        // 7 - when the processing finishes with success, the engine usess the changelog from step 6 so that
        //     the changes are translated to the immutable state
        // 8 - when step 7 completes, the engine raises an event with the changelog - 
        //     this is the hooking point for replicating the changelog on another machine



    let inline private _mapHierarchyError (he: Hierarchy.Error) =
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
                let clg = stateData.ChangeLog.Add (ViewAdded viewID)
                Ok ({ Hierarchy = hs; ViewModels = vms; ViewStates = vss; ChangeLog = clg })
            | None -> Error (ViewNotFound viewName)
        | None -> (Error (ExpectedViewKey guid))

    let private _updateViewModel (guid: Guid) (vm: obj) (sd: StateData): Result<StateData, Error> =
        match (Hierarchy.getViewID guid sd.Hierarchy) with
        | Some viewID ->
            let vm = sd.ViewModels.Remove(guid).Add(guid, vm)
            let cl = sd.ChangeLog.Add (ViewNeedsRefresh viewID)
            Ok { Hierarchy = sd.Hierarchy; ViewModels = vm; ViewStates = sd.ViewStates; ChangeLog = cl }
        | None -> (Error (ExpectedViewKey guid))

    let private _destroyView (guid: Guid) (sd: StateData): Result<StateData, Error> =
        match (Hierarchy.getViewID guid sd.Hierarchy) with
        | Some viewID ->
            match Hierarchy.remove (Hierarchy.Key.ViewKey viewID) sd.Hierarchy with
            | Ok (hierarchyUpdateResult) ->
                let (h, guids) = hierarchyUpdateResult
                let mutable (vm, vs, cl) = (sd.ViewModels, sd.ViewStates, sd.ChangeLog)
                for removedGuid in guids do
                    vm <- vm.Remove removedGuid
                    vs <- vs.Remove removedGuid
                    cl <- 
                        match Hierarchy.getViewID removedGuid sd.Hierarchy with
                        | Some removedViewID -> sd.ChangeLog.Add (ViewDestroyed removedViewID)
                        | None -> cl
                Ok { Hierarchy = h; ViewModels = vm; ViewStates = vs; ChangeLog = cl }
            | Error e -> e |> _mapHierarchyError |> Error // TODO map error
        | None -> (Error (ExpectedViewKey guid))

    let private _destroyRegion (guid: Guid) (sd: StateData): Result<StateData, Error> =
        match (Hierarchy.getRegionID guid sd.Hierarchy) with
        | Some regionID ->
            match Hierarchy.remove (Hierarchy.Key.RegionKey regionID) sd.Hierarchy with
            | Ok (hierarchyUpdateResult) ->
                let (h, guids) = hierarchyUpdateResult
                let mutable (vm, vs, cl) = (sd.ViewModels, sd.ViewStates, sd.ChangeLog)
                for removedGuid in guids do
                    vm <- vm.Remove removedGuid
                    vs <- vs.Remove removedGuid
                    cl <- 
                        match Hierarchy.getViewID removedGuid sd.Hierarchy with
                        | Some removedViewID -> sd.ChangeLog.Add (ViewDestroyed removedViewID)
                        | None -> cl
                Ok { Hierarchy = h; ViewModels = vm; ViewStates = vs; ChangeLog = cl }
            | Error e -> e |> _mapHierarchyError |> Error // TODO map error
        | None -> (Error (ExpectedViewKey guid))

    //let private _executCommand (guid: Guid) (name: string) (sd: StateData): Result<StateData, Error> =
    //    // TODO: need to handle a special case with command altering the hierarchy -- adding or deleting a view
    //    match sd.ViewStates.TryFind guid with
    //    | Some vs ->
    //        match vs.Descriptor.Commands.TryFind name with
    //        | Some cmd ->
    //            // TODO
    //        | None -> Error (CommandNotFound(Unchecked.defaultof<ViewID>, name))
    //    | None -> (Error (ExpectedViewKey guid))

    // -----------------------------------------

    let rec private _processChanges (ctx: IForestContext) (eventBus: IEventBus) (operation: ForestOperation) (stateData: StateData) =
        match operation with
        | Multiple operations -> _loopStates ctx eventBus operations stateData
        | InstantiateView (region, viewName) ->
            let vk = Hierarchy.Key.ViewKey (ViewID(region, -1, viewName))
            Ok stateData.Hierarchy
            |><| (Hierarchy.add vk, _mapHierarchyError)
            |>| _addViewState ctx eventBus stateData
        | UpdateViewModel (viewID, vm) -> _updateViewModel viewID vm stateData
        | DestroyView viewID -> _destroyView viewID stateData
        | DestroyRegion regionID -> _destroyRegion regionID stateData
        //| InvokeCommand (viewID, commandName, arg) -> Ok stateData >>= _executeCommand viewID commandName arg
        | _ -> Error (UnknownOperation operation)
    and private _loopStates ctx eventBus ops stateData =
        match ops with
        | [] -> Ok stateData
        | [op] -> _processChanges ctx eventBus op stateData
        | head::tail ->
            Ok stateData
            |>| _processChanges ctx eventBus head
            |>| _loopStates ctx eventBus tail

    type [<Sealed>] private T  =
        // transferable across machines
        val mutable private _hierarchy: Hierarchy.State
        // transferable across machines
        val mutable private _viewModels: Map<Guid, obj>
        // transferable across machines
        val mutable private _changeLog: StateChange Set

        // non-transferable across machines
        val mutable private _viewStates:  Map<Guid, ViewState>

        member this.Update (ctx: IForestContext, operation: ForestOperation) =
            let getState () = 
                { 
                    Hierarchy = this._hierarchy; 
                    ViewModels = this._viewModels; 
                    ViewStates = this._viewStates; 
                    ChangeLog = this._changeLog;
                }
            let swapState sd =
                this._hierarchy <- sd.Hierarchy
                this._viewModels <- sd.ViewModels
                this._viewStates <- sd.ViewStates
                this._changeLog <- sd.ChangeLog
                getState ()

            let sd = { 
                Hierarchy = this._hierarchy; 
                ViewModels = this._viewModels; 
                ViewStates = this._viewStates; 
                ChangeLog = this._changeLog;
            }
            use eventBus = EventBus.Create()
            match _processChanges ctx eventBus operation sd with
            | Ok state -> swapState state |> ignore
            | Error e ->
                // TODO exception
                ()
        interface IViewModelProvider with
            member this.GetViewModel guid = 
                this._viewModels.[guid]
            member this.SetViewModel guid value = 
                // TODO: context??
                this.Update(Unchecked.defaultof<IForestContext>, UpdateViewModel(guid, value))
