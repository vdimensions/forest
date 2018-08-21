namespace Forest

open System

[<Serializable>]
type [<Struct>] StateError =
    | ViewNotFound of ViewName: string
    | UnexpectedModelState of Path: Identifier
    | CommandNotFound of Parameters: Identifier * string
    | CommandError of Cause: Command.Error
    | HierarchyElementAbsent of ID: Identifier
    | NoViewAdded

[<Serializable>]
type StateChange =
    | ViewAdded of AddParams: Identifier * obj
    | ViewModelUpdated of UpdateParams: Identifier * obj
    | ViewDestroyed of DestroyedViewID: Identifier

type ForestOperation =
    | InstantiateView of Identifier * string * string
    | UpdateViewModel of Identifier * obj
    | DestroyView of Identifier
    | InvokeCommand of Identifier * string * obj
    | Multiple of ForestOperation list

type [<Sealed>] internal MutableState =
    interface IDisposable
    interface IViewStateModifier
    internal new: Hierarchy * Map<Identifier, obj> * Map<Identifier, ViewState> * IForestContext -> MutableState
    member Apply: bool -> StateChange -> StateError option
    member Update: ForestOperation -> StateChange list
    member Deconstruct: unit -> Hierarchy*Map<Identifier, obj>*Map<Identifier, ViewState>


[<Serializable>]
type State1

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