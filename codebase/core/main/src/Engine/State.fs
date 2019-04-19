namespace Forest

open Forest.Collections
open Forest.UI

open System

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type [<Sealed;NoComparison>] State internal(tree : Tree, models : Map<thash, obj>, viewStates :  Map<thash, IExecView>, physicalViews : Map<thash, IPhysicalView>, fuid: Fuid) =
    internal new (tree : Tree, viewModels : Map<thash, obj>, viewStates :  Map<thash, IExecView>, physicalViews : Map<thash, IPhysicalView>) = State(tree, viewModels, viewStates, physicalViews, Fuid.newID())
    [<CompiledName("Empty")>]
    static member initial = State(Tree.root, Map.empty, Map.empty, Map.empty, Fuid.empty)
    member internal __.Tree with get() = tree
    member internal __.Models with get() = models
    member internal __.ViewStates with get() = viewStates
    member internal __.PhysicalViews with get() = physicalViews
    member internal __.Fuid with get() = fuid
    member __.Hash with get() = fuid.Hash
    //member __.MachineToken with get() = fuid.MachineToken
    member private this.eq (other : State) : bool =
        StringComparer.Ordinal.Equals(this.Hash, other.Hash)
        && LanguagePrimitives.GenericEqualityComparer.Equals(this.Tree, other.Tree)
        && System.Object.Equals(this.Models, other.Models)
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
    let discardViewStates (st : State) = State(st.Tree, st.Models, Map.empty, Map.empty)

    let rec private _traverseState (v : IForestStateVisitor) parent (ids : TreeNode list) (siblingsCount : int) (st : State) =
        match ids with
        | [] -> ()
        | head::tail ->
            let ix = siblingsCount - ids.Length // TODO
            let hash = head.Hash
            let vm = st.Models.[hash]
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
