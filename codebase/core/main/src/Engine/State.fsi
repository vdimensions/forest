namespace Forest

open System

[<Serializable>]
type [<Struct>] StateError =
    | ViewNotFound of view: string
    | UnexpectedModelState of identifier: Identifier
    | CommandNotFound of owner: Identifier * command: string
    | CommandError of cause: Command.Error
    | HierarchyElementAbsent of orphanIdentifier: Identifier
    | NoViewAdded

[<Serializable>]
type StateChange =
    | ViewAdded of parent: Identifier * viewModel: obj
    | ViewModelUpdated of parent: Identifier * updatedViewModel: obj
    | ViewDestroyed of destroyedViewID: Identifier

type ForestOperation =
    | InstantiateView of parent: Identifier * region: string * viewName: string
    | UpdateViewModel of parent: Identifier * viewModel: obj
    | DestroyView of identifier: Identifier
    | InvokeCommand of owner: Identifier * commandName: string * commandArg: obj
    | Multiple of operations: ForestOperation list

type [<Sealed>] internal MutableStateScope =
    interface IDisposable
    interface IViewStateModifier
    internal new: Hierarchy * Map<Identifier, obj> * Map<Identifier, ViewState> * IForestContext -> MutableStateScope
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