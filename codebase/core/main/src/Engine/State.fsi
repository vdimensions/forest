namespace Forest

open System

[<Serializable>]
type [<Struct>] StateError =
    | ViewNotFound of view:string
    | UnexpectedModelState of identifier:HierarchyKey
    | CommandNotFound of owner:HierarchyKey * command:string
    | CommandError of cause:Command.Error
    | HierarchyElementAbsent of orphanIdentifier:HierarchyKey
    | NoViewAdded

[<Serializable>]
type [<Struct>] StateChange =
    | ViewAdded of parent:HierarchyKey * viewModel:obj
    | ViewModelUpdated of id:HierarchyKey * updatedViewModel:obj
    | ViewDestroyed of destroyedViewID:HierarchyKey

[<Serializable>]
type [<Sealed>] State = // TODO: convert to state machine
    internal new: Hierarchy * Map<HierarchyKey, obj> * Map<HierarchyKey, IViewState> -> State
    [<CompiledName("Empty")>]
    static member empty:State
    member internal Hierarchy:Hierarchy with get
    member internal ViewModels:Map<HierarchyKey, obj> with get
    member internal ViewStates:Map<HierarchyKey, IViewState> with get
    [<System.Diagnostics.DebuggerNonUserCode>]
    member internal Fuid:Fuid with get
    member Hash:string with get
    //member MachineToken: string with get
    interface IEquatable<State>

[<RequireQualifiedAccess>]
module internal State =
    val create: Hierarchy * Map<HierarchyKey, obj> * Map<HierarchyKey, IViewState> -> State
    val createWithFuid: Hierarchy * Map<HierarchyKey, obj> * Map<HierarchyKey, IViewState> * Fuid -> State
    val discardViewStates: State -> State