namespace Forest

open System

[<Serializable>]
type State =
    internal new: Hierarchy*Map<Identifier, obj>*Map<Identifier, ViewState> -> State
    member internal Hierarchy: Hierarchy with get
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

    val internal create: Hierarchy*Map<Identifier, obj>*Map<Identifier, ViewState> -> State

    val internal discardViewStates: State -> State

    [<CompiledName("Empty")>]
    val empty: State