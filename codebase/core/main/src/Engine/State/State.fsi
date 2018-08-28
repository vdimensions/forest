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

type [<Interface>] IStateVisitor =
    abstract member BFS: key:HierarchyKey -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    abstract member DFS: key:HierarchyKey -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    abstract member Done: unit -> unit

[<RequireQualifiedAccess>]
module internal State =
    val create: Hierarchy * Map<string, obj> * Map<string, IViewState> -> State
    val createWithFuid: Hierarchy * Map<string, obj> * Map<string, IViewState> * Fuid -> State
    val discardViewStates: State -> State
    val traverse: visitor:IStateVisitor -> st:State -> unit