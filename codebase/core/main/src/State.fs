namespace Forest

open System


type State internal(hierarchy: Hierarchy.State, viewModels: Map<Identifier, obj>, viewStates:  Map<Identifier, ViewState>) =
    member internal __.Hierarchy with get() = hierarchy
    member internal __.ViewModels with get() = viewModels
    member internal __.ViewStates with get() = viewStates

[<RequireQualifiedAccess>]
module State =
    [<Serializable>]
    [<RequireQualifiedAccess>]
    type StateChange =
        | ViewAdded of Identifier * obj
        | ViewModelUpdated of Identifier * obj
        | ViewDestroyed of Identifier

    [<CompiledName("Create")>]
    let internal create (hs, vm, vs) = State(hs, vm, vs)

    [<CompiledName("Empty")>]
    let empty = State(Hierarchy.empty, Map.empty, Map.empty)
