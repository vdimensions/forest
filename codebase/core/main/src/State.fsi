namespace Forest

open System

[<Serializable>]
type StateError =
    | ViewNotFound of ViewName: string
    | UnexpectedModelState of Path: Identifier
    | CommandNotFound of Parameters: Identifier * string
    | CommandError of Cause: Command.Error
    | HierarchyElementAbsent of id: Identifier
    | NoViewAdded
    
[<Serializable>]
type StateChange =
    | ViewAdded of Identifier * obj
    | ViewModelUpdated of Identifier * obj
    | ViewDestroyed of Identifier

[<Serializable>]
type State = // TODO: convert to state machine
    internal new: Hierarchy*Map<Identifier, obj>*Map<Identifier, ViewState> -> State
    [<CompiledName("Empty")>]
    static member empty: State
    member internal Hierarchy: Hierarchy with get
    member internal ViewModels: Map<Identifier, obj> with get
    member internal ViewStates: Map<Identifier, ViewState> with get

[<RequireQualifiedAccess>]
module State =
    val internal create: Hierarchy*Map<Identifier, obj>*Map<Identifier, ViewState> -> State

    val internal discardViewStates: State -> State