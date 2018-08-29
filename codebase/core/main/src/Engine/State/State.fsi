namespace Forest

open System

[<Serializable>]
type [<Struct>] StateChange =
    | ViewAdded of parent:HierarchyKey * viewModel:obj
    | ViewModelUpdated of id:HierarchyKey * updatedViewModel:obj
    | ViewDestroyed of destroyedViewID:HierarchyKey

[<Serializable>]
type [<Sealed>] State =
    internal new: Hierarchy * Map<string, obj> * Map<string, IViewState> -> State
    [<CompiledName("Empty")>]
    static member empty:State
    member internal Hierarchy:Hierarchy with get
    member internal ViewModels:Map<string, obj> with get
    member internal ViewStates:Map<string, IViewState> with get
    member internal Fuid:Fuid with get
    member Hash:string with get
    //member MachineToken: string with get
    interface IEquatable<State>

/// An interface representing a forest state visitor;
type [<Interface>] IForestStateVisitor =
    abstract member BFS: key:HierarchyKey -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    abstract member DFS: key:HierarchyKey -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    /// Executed once when the traversal is complete.
    abstract member Complete: unit -> unit

[<RequireQualifiedAccess>]
module State =
    val internal create: Hierarchy * Map<string, obj> * Map<string, IViewState> -> State
    val internal createWithFuid: Hierarchy * Map<string, obj> * Map<string, IViewState> * Fuid -> State
    val internal discardViewStates: State -> State
    [<CompiledName("Traverse")>]
    val traverse: visitor:IForestStateVisitor -> st:State -> unit