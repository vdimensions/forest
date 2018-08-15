namespace Forest

open System


type State internal(hierarchy: Hierarchy.State, viewModels: Map<Guid, obj>, viewStates:  Map<Guid, ViewState>) =
    member internal __.Hierarchy with get() = hierarchy
    member internal __.ViewModels with get() = viewModels
    member internal __.ViewStates with get() = viewStates

[<RequireQualifiedAccess>]
module State =
    [<Serializable>]
    [<RequireQualifiedAccess>]
    type StateChange =
        | ViewAdded of ViewID * Guid * obj
        | ViewModelUpdated of ViewID * Guid * obj
        | ViewDestroyed of ViewID * Guid

    [<CompiledName("Create")>]
    let internal create (hs, vm, vs) = State(hs, vm, vs)

    [<CompiledName("Empty")>]
    let empty = State(Hierarchy.empty, Map.empty, Map.empty)
