namespace Forest

open Forest.Collections

open System

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type [<Sealed;NoComparison>] internal State internal(tree : Tree, viewModels : Map<thash, obj>, viewStates :  Map<thash, IRuntimeView>, fuid: Fuid) =
    internal new (tree : Tree, viewModels : Map<thash, obj>, viewStates :  Map<thash, IRuntimeView>) = State(tree, viewModels, viewStates, Fuid.newID())
    [<CompiledName("Empty")>]
    static member initial = State(Tree.root, Map.empty, Map.empty, Fuid.empty)
    member internal __.Tree with get() = tree
    member internal __.ViewModels with get() = viewModels
    member internal __.ViewStates with get() = viewStates
    member internal __.Fuid with get() = fuid
    member __.Hash with get() = fuid.Hash
    //member __.MachineToken with get() = fuid.MachineToken
    member private this.eq (other : State) : bool =
        StringComparer.Ordinal.Equals(this.Hash, other.Hash)
        && LanguagePrimitives.GenericEqualityComparer.Equals(this.Tree, other.Tree)
        && System.Object.Equals(this.ViewModels, other.ViewModels)
    override this.Equals(o : obj):bool =
        match o with
        | :? State as other -> this.eq other
        | _ -> false
    override this.GetHashCode() = this.Hash.GetHashCode()
    interface IEquatable<State> with member this.Equals(other:State) = this.eq other

[<RequireQualifiedAccess>]
module internal State =
    let create (hs, vm, vs) = State(hs, vm, vs)
    let createWithFuid (hs, vm, vs, fuid) = State(hs, vm, vs, fuid)
    let discardViewStates (st : State) = State(st.Tree, st.ViewModels, Map.empty)

    let rec private _traverseState (v : IForestStateVisitor) parent (ids : TreeNode list) (siblingsCount : int) (st : State) =
        match ids with
        | [] -> ()
        | head::tail ->
            let ix = siblingsCount - ids.Length // TODO
            let hash = head.Hash
            let vm = st.ViewModels.[hash]
            let vs = st.ViewStates.[hash]
            let descriptor = vs.Descriptor
            v.BFS head ix vm descriptor
            // visit siblings 
            _traverseState v parent tail siblingsCount st
            // visit children
            match st.Tree.Hierarchy.TryFind head with
            | Some children -> _traverseState v head (children |> List.rev) children.Length st
            | None -> ()
            v.DFS head ix vm descriptor
            ()

    [<CompiledName("Traverse")>]
    let traverse (v : IForestStateVisitor) (st : State) =
        let root = TreeNode.shell
        match st.Tree.Hierarchy.TryFind root with
        | Some ch -> _traverseState v root ch ch.Length st
        | None -> ()
        v.Complete()


