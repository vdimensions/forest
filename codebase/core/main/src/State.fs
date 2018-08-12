namespace Forest

open Forest

open System


// contains the active mutable forest state, such as the latest dom index and view state changes
type [<Sealed>] internal ViewState(id: Guid, descriptor: IViewDescriptor, viewInstance: IViewInternal) =
    member __.ID with get() = id
    member __.Descriptor with get() = descriptor
    member __.View with get() = viewInstance

  
module State =

    [<RequireQualifiedAccess>]
    type StateChange =
        | ViewAdded of ViewID * Guid * obj
        | ViewModelUpdated of ViewID * Guid * obj
        | ViewDestroyed of ViewID * Guid

    type [<Struct>] internal State = {
        Hierarchy: Hierarchy.State;
        // transferable across machines
        ViewModels: Map<Guid, obj>;
        // non-transferable across machines
        ViewStates:  Map<Guid, ViewState>
    }

    let internal empty: State = { Hierarchy = Hierarchy.empty; ViewModels = Map.empty; ViewStates = Map.empty }

    //let Create() = new T()
