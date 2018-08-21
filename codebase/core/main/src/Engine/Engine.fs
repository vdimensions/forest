namespace Forest

open Forest


[<RequireQualifiedAccess>]
module Engine =
    let inline private _applyStateChanges (mutableState:MutableStateScope) (sync: bool) (changeLog: StateChange List) (ctx: IForestContext) (state: State) =
        let rec _applyChangelog (ms: MutableStateScope) (cl: StateChange List) =
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


    let ApplyChangeLog (ctx: IForestContext) (state: State) (changeLog: StateChange List) = 
        use ms = new MutableStateScope(state.Hierarchy, state.ViewModels, state.ViewStates, ctx)
        _applyStateChanges ms true changeLog ctx state

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
        use ms = new MutableStateScope(state.Hierarchy, state.ViewModels, state.ViewStates, ctx)
        let c = ms.Update operation
        //_applyStateChanges ms false c ctx state
        // TODO: raise event for state changes
        State.create <| ms.Deconstruct()
