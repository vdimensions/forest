﻿namespace Forest

open System
open System.Diagnostics

[<Serializable>]
type [<Struct>] StateChange =
    | ViewAdded of parent:HierarchyKey * viewModel:obj
    | ViewModelUpdated of id:HierarchyKey * updatedViewModel:obj
    | ViewDestroyed of destroyedViewID:HierarchyKey

[<Serializable>]
type [<Sealed>] State internal(hierarchy: Hierarchy, viewModels: Map<string, obj>, viewStates:  Map<string, IViewState>, fuid: Fuid) =
    internal new (hierarchy: Hierarchy, viewModels: Map<string, obj>, viewStates:  Map<string, IViewState>) = State(hierarchy, viewModels, viewStates, Fuid.newID())
    [<CompiledName("Empty")>]
    static member empty = State(Hierarchy.empty, Map.empty, Map.empty, Fuid.empty)
    member internal __.Hierarchy with get() = hierarchy
    member internal __.ViewModels with get() = viewModels
    member internal __.ViewStates with get() = viewStates
    [<DebuggerNonUserCode>]
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
