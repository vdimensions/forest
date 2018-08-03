namespace Forest

open Forest
open Forest.Events

open System


type RegionID = Root | Region of ViewID * string
 and ViewID = RegionID * string * int

// ----------------------------------

type ForestOperation =
    | InstantiateView of RegionID * string
    | UpdateViewModel of ViewID * obj
    | ReorderView of ViewID
    | HandleEvent of ViewID
    | DestroyView of ViewID
    | DestroyRegion of RegionID
    | InvokeCommand of ViewID * string * obj
    | Multiple of ForestOperation list

//type internal ForestStateData =
//    | ViewState of ViewID * IViewDescriptor * IViewInternal
//    | RegionState of RegionID
    
// contains the active mutable forest state, such as the latest dom index and view state changes

type [<Sealed>] internal ViewState(id: ViewID, descriptor: IViewDescriptor, viewInstance: IViewInternal) =
    member this.ID with get() = id
    member this.Descriptor with get() = descriptor
    member this.View with get() = viewInstance
   

module State =
    type Error =
        | ViewNotFound of string
        | UnexpectedModelState of ViewID
        | MissingModelState of ViewID
        | CommandNotFound of ViewID * string
        | CommandBadArgument of ViewID * string * Type
        | UnknownOperation of ForestOperation

    type StateChange =
        | ViewAdded of ViewID
        | ViewNeedsRefresh of ViewID
        | ViewDestroyed of ViewID

    type private StateData = { 
        VHierarchy: Map<RegionID, ViewID list>;
        RHierarchy: Map<ViewID, RegionID Set>;
        ModelData: Map<ViewID, obj>; 
        ViewStateData: Map<ViewID, ViewState>
        ChangeLog: StateChange Set
    }

    // -----------------------------------------
    let inline private _addViewModel (viewID: ViewID) (view: IViewInternal) (sd: StateData): Result<StateData, Error> =
        let (regionID, _, _) = viewID
        
        if not (sd.ModelData.ContainsKey viewID) then
            let vh = 
                if not (sd.VHierarchy.ContainsKey regionID) 
                then sd.VHierarchy.Add (regionID, [viewID])
                else sd.VHierarchy.Add (regionID, (sd.VHierarchy.[regionID] @ [viewID]) )

            let rh = 
                if not (sd.RHierarchy.ContainsKey viewID) 
                then sd.RHierarchy.Add (viewID, Set.empty) 
                else sd.RHierarchy

            let newVMData = sd.ModelData.Add (viewID, view.ViewModel)
            Success {
                VHierarchy = vh;
                RHierarchy = rh;
                ModelData = newVMData;
                ViewStateData = sd.ViewStateData;
                ChangeLog = sd.ChangeLog.Add (ViewAdded viewID);
            }
        else Failure (UnexpectedModelState viewID)

    let inline private _addOrUpdateViewModel (viewID: ViewID) (viewModel: obj) (sd: StateData): Result<StateData, Error> =
        let (regionID, _, _) = viewID
        let mutable cl = sd.ChangeLog
        // TODO: check hierarchy consistency
        let vh = 
            if not (sd.VHierarchy.ContainsKey regionID) 
            then sd.VHierarchy.Add (regionID, [viewID])
            else sd.VHierarchy.Add (regionID, (sd.VHierarchy.[regionID] @ [viewID]) )

        let rh = 
            if not (sd.RHierarchy.ContainsKey viewID) 
            then sd.RHierarchy.Add (viewID, Set.empty) 
            else sd.RHierarchy

        let md = (
            if (sd.ModelData.ContainsKey viewID) 
            then
                cl <- cl.Add (ViewNeedsRefresh viewID)
                sd.ModelData.Remove viewID
            else 
                cl <- cl.Add (ViewAdded viewID)
                sd.ModelData).Add (viewID, viewModel)
        
        Success { 
            VHierarchy = vh;
            RHierarchy = rh;
            ModelData = md;
            ViewStateData = sd.ViewStateData;
            ChangeLog = cl;
        }

    let inline private _updateViewModel (viewID: ViewID) (viewModel: obj) (sd: StateData): Result<StateData, Error> =
        let (regionID, _, _) = viewID
        // TODO: check hierarchy consistency

        if (sd.ModelData.ContainsKey viewID) 
        then
            let vh = 
                if not (sd.VHierarchy.ContainsKey regionID) 
                then sd.VHierarchy.Add (regionID, [viewID])
                else sd.VHierarchy.Add (regionID, (sd.VHierarchy.[regionID] @ [viewID]) )

            let rh = 
                if not (sd.RHierarchy.ContainsKey viewID) 
                then sd.RHierarchy.Add (viewID, Set.empty) 
                else sd.RHierarchy
                
            Success { 
                VHierarchy = vh;
                RHierarchy = rh;
                ModelData = (sd.ModelData.Remove viewID).Add (viewID, viewModel);
                ViewStateData = sd.ViewStateData;
                ChangeLog = sd.ChangeLog.Add (ViewNeedsRefresh viewID);
            }
        else Failure (MissingModelState viewID)

    let inline private _addOrUpdateViewState (instanceID: ViewID) (viewState: ViewState) (sd: StateData): Result<StateData, Error> =
        // TODO: check hierarchy consistency
        let vs = (
            if (sd.ViewStateData.ContainsKey instanceID)
            then sd.ViewStateData.Remove instanceID
            else sd.ViewStateData).Add(instanceID, viewState)

        Success { 
            VHierarchy = sd.VHierarchy; 
            RHierarchy = sd.RHierarchy; 
            ModelData = sd.ModelData; 
            ViewStateData = vs; 
            ChangeLog = sd.ChangeLog; 
        }

    let rec private _destroyViews (viewIDs: ViewID list) (sd: StateData): StateData =
        match viewIDs with
        | [] -> sd
        | head::tail ->
            let nsd = 
                match (sd.RHierarchy.TryFind head) with
                | Some regionIDs -> _destroyRegions (List.ofSeq regionIDs) sd
                | None -> sd

            let vs = 
                match nsd.ViewStateData.TryFind head with
                | Some viewState ->
                    match viewState.View with 
                    | :? IDisposable as d -> 
                        // TODO: could throw exception
                        d.Dispose()
                    | _ -> ()
                    nsd.ViewStateData.Remove head
                | None -> nsd.ViewStateData

            let vm =
                match nsd.ModelData.TryFind head with
                | Some _ -> nsd.ModelData.Remove head
                | None -> nsd.ModelData

            let (parentRegion, _, _) = head
            let vh = 
                match (nsd.VHierarchy.TryFind parentRegion) with
                // TODO: `List.except` could be slow
                | Some viewIDs -> (nsd.VHierarchy.Remove parentRegion).Add (parentRegion, (viewIDs |> List.except (Seq.ofArray [|head|]) ) )
                | None -> nsd.VHierarchy
            let rh = nsd.RHierarchy
            let cl = nsd.ChangeLog.Add (ViewDestroyed head)

            _destroyViews tail { VHierarchy = vh; RHierarchy = rh; ModelData = vm; ViewStateData = vs; ChangeLog = cl; }

    and private _destroyRegions (regionIDs: RegionID list) (sd: StateData): StateData =
        match regionIDs with
        | [] -> sd
        | head::tail -> 
            let nsd = 
                match (sd.VHierarchy.TryFind head) with
                | Some viewIDs -> _destroyViews viewIDs sd
                | None -> sd

            let vh = nsd.VHierarchy
            let cl = nsd.ChangeLog
            let vm = nsd.ModelData
            let vs = nsd.ViewStateData

            let rh = 
                match head with
                | Root -> nsd.RHierarchy
                | Region (ownerViewID, _) ->
                    match (nsd.RHierarchy.TryFind ownerViewID) with
                    // TODO: `List.except` could be slow
                    | Some regionIDs -> (nsd.RHierarchy.Remove ownerViewID).Add (ownerViewID, (regionIDs |> Set.remove (head) ) )
                    | None -> nsd.RHierarchy

            _destroyRegions tail { VHierarchy = vh; RHierarchy = rh; ModelData = vm; ViewStateData = vs; ChangeLog = cl; }


    let inline private _executeCommand (viewID: ViewID) (commandName: string) (arg) (sd: StateData): Result<StateData, Error> =
        match sd.ViewStateData.TryFind viewID with
        | Some viewState -> 
            let descriptor = viewState.Descriptor
            // TODO: implement command lookup
            Success sd
        | None -> Failure (CommandNotFound (viewID, commandName))
        
    // -----------------------------------------

    let rec private _processChanges(ctx: IForestContext, eventBus: IEventBus, stateData: StateData, operation: ForestOperation): Result<StateData, Error> =
        match operation with
        | Multiple operations -> 
            let rec loopStates(c, eb, sd, op) =
                match op with
                | [] -> Success sd
                | [singleOperation] -> _processChanges(c, eb, sd, singleOperation)
                | head::tail ->
                    match _processChanges(c, eb, sd, head) with
                    | Success tmp -> loopStates(c, eb, tmp, tail)
                    | Failure e -> Failure e
            loopStates(ctx, eventBus, stateData, operations)
        | InstantiateView (region, viewID) ->
            let instanceID: ViewID = (region, viewID, -1)
            match ctx.ViewRegistry.GetViewMetadata(viewID) with
            | Some descriptor ->
                let viewInstance = (ctx.ViewRegistry.Resolve viewID) :?> IViewInternal
                viewInstance.EventBus <- eventBus
                // TODO:
                //viewInstance.InstanceID <- ({ ID = { Value = v } } instanceID)

                let viewState = ViewState(instanceID, descriptor, viewInstance)
                // TODO:


                Success stateData 
                >>= _addViewModel instanceID viewInstance
                >>= _addOrUpdateViewState instanceID viewState
            | None -> Failure (ViewNotFound viewID)
        | UpdateViewModel (viewID, vm) -> Success stateData >>= _updateViewModel viewID vm
        | DestroyView viewID -> Success (stateData |> _destroyViews [viewID])
        | DestroyRegion regionID -> Success (stateData |> _destroyRegions [regionID])
        | InvokeCommand (viewID, commandName, arg) -> Success stateData >>= _executeCommand viewID commandName arg
        | _ -> Failure (UnknownOperation operation)

    type [<Sealed>] private T =
        // transferable across machines
        val mutable private _vhierarchy: Map<RegionID, ViewID list>
        // transferable across machines
        val mutable private _rhierarchy: Map<ViewID, RegionID Set>
        // transferable across machines
        val mutable private _viewModelData: Map<ViewID, obj>
        // transferable across machines
        val mutable private _changeLog: StateChange Set
        // non-transferable across machines
        val mutable private _viewStateData: Map<ViewID, ViewState>

        member this.Update (ctx: IForestContext, operation: ForestOperation) =
            let sd = { 
                VHierarchy = this._vhierarchy; 
                RHierarchy = this._rhierarchy; 
                ModelData = this._viewModelData; 
                ViewStateData = this._viewStateData; 
                ChangeLog = this._changeLog;
            }
            use eventBus = EventBus.Create()
            match _processChanges(ctx, eventBus, sd, operation) with
            | Success state ->
                this._vhierarchy <- state.VHierarchy
                this._rhierarchy <- state.RHierarchy
                this._viewModelData <- state.ModelData
                this._viewStateData <- state.ViewStateData
                this._changeLog <- state.ChangeLog
                ()
            | Failure e ->
                // TODO exception
                ()
