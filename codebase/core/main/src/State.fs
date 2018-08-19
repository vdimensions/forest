namespace Forest

open System


[<Serializable>]
type StateError =
    | ViewNotFound of ViewName: string
    | UnexpectedModelState of Path: Identifier
    | CommandNotFound of Parameters: Identifier * string
    //| CommandBadArgument of Parameters: Identifier * string * Type
    | CommandError of Cause: Command.Error
    | HierarchyElementAbsent of id: Identifier
    | NoViewAdded


[<Serializable>]
type StateChange =
    | ViewAdded of Identifier * obj
    | ViewModelUpdated of Identifier * obj
    | ViewDestroyed of Identifier

[<Serializable>]
type State internal(hierarchy: Hierarchy, viewModels: Map<Identifier, obj>, viewStates:  Map<Identifier, ViewState>) =
    [<CompiledName("Empty")>]
    static member empty = State(Hierarchy.empty, Map.empty, Map.empty)
    member internal __.Hierarchy with get() = hierarchy
    member internal __.ViewModels with get() = viewModels
    member internal __.ViewStates with get() = viewStates

[<RequireQualifiedAccess>]
module internal State =
    let create (hs, vm, vs) = 
        State(hs, vm, vs)

    let discardViewStates (st: State) =
        State(st.Hierarchy, st.ViewModels, Map.empty)
