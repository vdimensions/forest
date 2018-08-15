namespace Forest

open System

[<Serializable>]
type State =
    internal new: Hierarchy.State*Map<Guid, obj>*Map<Guid, ViewState> -> State
    member internal Hierarchy: Hierarchy.State with get
    member internal ViewModels: Map<Guid, obj> with get
    member internal ViewStates: Map<Guid, ViewState> with get


[<RequireQualifiedAccess>]
module State =
    [<Serializable>]
    [<RequireQualifiedAccess>]
    type StateChange =
        | ViewAdded of ViewID * Guid * obj
        | ViewModelUpdated of ViewID * Guid * obj
        | ViewDestroyed of ViewID * Guid

    [<CompiledName("Create")>]
    val internal create: Hierarchy.State*Map<Guid, obj>*Map<Guid, ViewState> -> State

    [<CompiledName("Empty")>]
    val empty: State