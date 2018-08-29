namespace Forest

open Forest.Collections

open System

[<Serializable>]
type [<Sealed>] internal State internal(hierarchy:Tree, viewModels: Map<string, obj>, viewStates:  Map<string, IViewState>, fuid: Fuid) =
    internal new (hierarchy:Tree, viewModels: Map<string, obj>, viewStates:  Map<string, IViewState>) = State(hierarchy, viewModels, viewStates, Fuid.newID())
    [<CompiledName("Empty")>]
    static member empty = State(Tree.empty, Map.empty, Map.empty, Fuid.empty)
    member internal __.Hierarchy with get() = hierarchy
    member internal __.ViewModels with get() = viewModels
    member internal __.ViewStates with get() = viewStates
    member internal __.Fuid with get() = fuid
    member __.Hash with get() = fuid.Hash
    //member __.MachineToken with get() = fuid.MachineToken
    member private this.eq (other:State):bool =
        StringComparer.Ordinal.Equals(this.Hash, other.Hash)
        && LanguagePrimitives.GenericEqualityComparer.Equals(this.Hierarchy, other.Hierarchy)
        && System.Object.Equals(this.ViewModels, other.ViewModels)
    override this.Equals(o:obj):bool =
        match o with
        | :? State as other -> this.eq other
        | _ -> false
    override this.GetHashCode() = hash this.Hash
    interface IEquatable<State> with member this.Equals(other:State) = this.eq other

[<RequireQualifiedAccess>]
module internal State =
    let create (hs, vm, vs) = State(hs, vm, vs)
    let createWithFuid (hs, vm, vs, fuid) = State(hs, vm, vs, fuid)
    let discardViewStates (st: State) = State(st.Hierarchy, st.ViewModels, Map.empty)

    let rec private _traverseState (v:IForestStateVisitor) parent (ids:HierarchyKey list) (siblingsCount:int) (st:State) =
        match ids |> List.rev with
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
            match st.Hierarchy.Hierarchy.TryFind head with
            | Some children -> _traverseState v head children children.Length st
            | None -> ()
            v.DFS head ix vm descriptor
            ()

    [<CompiledName("Traverse")>]
    let traverse (v: IForestStateVisitor) (st: State) =
        let root = HierarchyKey.shell
        match st.Hierarchy.Hierarchy.TryFind root with
        | Some ch -> _traverseState v root ch ch.Length st
        | None -> ()
        v.Complete()


