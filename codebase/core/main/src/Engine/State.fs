namespace Forest

open System
open System.Collections.Immutable
open Forest
open Forest.Engine
open Forest.UI
open Forest.StateManagement

[<RequireQualifiedAccess>]
module internal State =
    let create (hs, m, vs, pv) = ForestState((GuidGenerator.NewID()), hs, m, vs, pv)
    let createWithFuid (hs, m, vs, pv, fuid) = ForestState(fuid, hs, m, vs, pv)
    let discardViewStates (st : ForestState) = ForestState(st.StateID, st.Tree, st.ViewStates, ImmutableDictionary<thash, IRuntimeView>.Empty, ImmutableDictionary<thash, IPhysicalView>.Empty)

    let rec private _traverseState (v : IForestStateVisitor) parent (ids : Tree.Node list) (siblingsCount : int) (st : ForestState) =
        match ids with
        | [] -> ()
        | head::tail ->
            let ix = siblingsCount - ids.Length // TODO
            let instanceID = head.InstanceID
            let viewState = st.ViewStates.[instanceID]
            let vs = st.LogicalViews.[instanceID]
            let descriptor = vs.Descriptor
            v.BFS (head, ix, viewState, descriptor)
            // visit siblings 
            _traverseState v parent tail siblingsCount st
            // visit children
            match st.Tree.[head] |> List.ofSeq with
            | [] -> ()
            | children -> _traverseState v head (children) children.Length st
            v.DFS (head, ix, viewState, descriptor)
            ()

    [<CompiledName("Traverse")>]
    let traverse (v : IForestStateVisitor) (st : ForestState) =
        let root = Tree.Node.Shell
        match st.Tree.[root] |> List.ofSeq with
        | [] -> ()
        | ch -> _traverseState v root ch ch.Length st
        v.Complete()

type [<Sealed;NoComparison;NoEquality>] DefaultForestStateProvider() =
    [<DefaultValue>]
    val mutable private _st : ForestState voption

    member this.LoadState () =
        match this._st with
        | ValueNone -> 
            let res = ForestState()
            this._st <- ValueSome res
            res
        | ValueSome s -> s

    member this.CommitState state = this._st <- ValueSome state

    interface IForestStateProvider with
        member this.LoadState() = this.LoadState()
        member this.CommitState(state) = this.CommitState(state)
        member __.RollbackState() = ()