namespace Forest

open System

[<Serializable>]
type [<Struct>] StateChange =
    | ViewAdded of parent:HierarchyKey * viewModel:obj
    | ViewModelUpdated of id:HierarchyKey * updatedViewModel:obj
    | ViewDestroyed of destroyedViewID:HierarchyKey

[<Serializable>]
type [<Sealed>] State = // TODO: convert to state machine
    internal new: Hierarchy * Map<string, obj> * Map<string, IViewState> -> State
    [<CompiledName("Empty")>]
    static member empty:State
    member internal Hierarchy:Hierarchy with get
    member internal ViewModels:Map<string, obj> with get
    member internal ViewStates:Map<string, IViewState> with get
    [<System.Diagnostics.DebuggerNonUserCode>]
    member internal Fuid:Fuid with get
    member Hash:string with get
    //member MachineToken: string with get
    interface IEquatable<State>

[<RequireQualifiedAccess>]
module internal State =
    val create: Hierarchy * Map<string, obj> * Map<string, IViewState> -> State
    val createWithFuid: Hierarchy * Map<string, obj> * Map<string, IViewState> * Fuid -> State
    val discardViewStates: State -> State