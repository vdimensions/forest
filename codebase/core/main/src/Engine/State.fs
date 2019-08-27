namespace Forest

open System
open Forest
open Forest.UI

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type [<Sealed;NoComparison>] State 
        internal(tree : Tree, 
                 viewState : Map<thash, ViewState>, 
                 views : Map<thash, IRuntimeView>, 
                 physicalViews : Map<thash, IPhysicalView>, 
                 hash: thash) =
    internal new (tree : Tree, viewModels : Map<thash, ViewState>, viewStates :  Map<thash, IRuntimeView>, physicalViews : Map<thash, IPhysicalView>) = State(tree, viewModels, viewStates, physicalViews, Fuid.newID().Hash)
    [<CompiledName("Empty")>]
    static member initial = State(Tree.Root, Map.empty, Map.empty, Map.empty, Fuid.empty.Hash)
    member internal __.Tree with get() = tree
    member internal __.ViewState with get() = viewState
    member internal __.Views with get() = views
    member internal __.PhysicalViews with get() = physicalViews
    member __.Hash with get() = hash
    member private this.eq (other : State) : bool =
        StringComparer.Ordinal.Equals(this.Hash, other.Hash)
        && LanguagePrimitives.GenericEqualityComparer.Equals(this.Tree, other.Tree)
        && System.Object.Equals(this.ViewState, other.ViewState)
    override this.Equals(o : obj):bool =
        match o with
        | :? State as other -> this.eq other
        | _ -> false
    override this.GetHashCode() = this.Hash.GetHashCode()
    interface IEquatable<State> with member this.Equals(other:State) = this.eq other

[<RequireQualifiedAccess>]
module internal State =
    let create (hs, m, vs, pv) = State(hs, m, vs, pv)
    let createWithFuid (hs, m, vs, pv, fuid) = State(hs, m, vs, pv, fuid)
    let discardViewStates (st : State) = State(st.Tree, st.ViewState, Map.empty, Map.empty)

    let rec private _traverseState (v : IForestStateVisitor) parent (ids : Tree.Node list) (siblingsCount : int) (st : State) =
        match ids with
        | [] -> ()
        | head::tail ->
            let ix = siblingsCount - ids.Length // TODO
            let hash = head.InstanceID
            let viewState = st.ViewState.[hash]
            let vs = st.Views.[hash]
            let descriptor = vs.Descriptor
            v.BFS head ix viewState descriptor
            // visit siblings 
            _traverseState v parent tail siblingsCount st
            // visit children
            match st.Tree.[head] |> List.ofSeq with
            | [] -> ()
            | children -> _traverseState v head (children) children.Length st
            v.DFS head ix viewState descriptor
            ()

    [<CompiledName("Traverse")>]
    let traverse (v : IForestStateVisitor) (st : State) =
        let root = Tree.Node.Shell
        match st.Tree.[root] |> List.ofSeq with
        | [] -> ()
        | ch -> _traverseState v root ch ch.Length st
        v.Complete()

type [<Interface>] IForestStateProvider =
    abstract member LoadState : unit -> State
    abstract member CommitState : State -> unit
    abstract member RollbackState : unit -> unit

type [<Sealed;NoComparison;NoEquality>] DefaultForestStateProvider() =
    [<DefaultValue>]
    val mutable private _st : State voption

    member this.LoadState () =
        match this._st with
        | ValueNone -> 
            let res = State.initial
            this._st <- ValueSome res
            res
        | ValueSome s -> s

    member this.CommitState state = this._st <- ValueSome state

    interface IForestStateProvider with
        member this.LoadState() = this.LoadState()
        member this.CommitState(state) = this.CommitState(state)
        member __.RollbackState() = ()