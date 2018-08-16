namespace Forest

open System

[<Serializable>]
type State =
    internal new: Hierarchy.State*Map<Identifier, obj>*Map<Identifier, ViewState> -> State
    member internal Hierarchy: Hierarchy.State with get
    member internal ViewModels: Map<Identifier, obj> with get
    member internal ViewStates: Map<Identifier, ViewState> with get


[<RequireQualifiedAccess>]
module State =
    [<Serializable>]
    [<RequireQualifiedAccess>]
    type StateChange =
        | ViewAdded of Identifier * obj
        | ViewModelUpdated of Identifier * obj
        | ViewDestroyed of Identifier

    [<CompiledName("Create")>]
    val internal create: Hierarchy.State*Map<Identifier, obj>*Map<Identifier, ViewState> -> State

    [<CompiledName("Empty")>]
    val empty: State