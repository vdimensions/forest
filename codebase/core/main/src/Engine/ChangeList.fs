namespace Forest
open Axle.Verification
open System.Collections
open System.Collections.Generic


#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
type [<Sealed;NoComparison>] ChangeList internal (previousStateHash : thash, changes : StateChange list, currentStateFuid : Fuid) =
    do
        ignore <| (|NotNull|) "previousStateHash" previousStateHash
        ignore <| (|NotNull|) "changes" changes
    member __.InHash with get() = previousStateHash
    member __.OutHash with get() = currentStateFuid.Hash
    member internal __.Fuid with get() = currentStateFuid
    member internal __.ToList() = changes
    interface IEnumerable<StateChange> with member __.GetEnumerator() = (upcast changes : IEnumerable<_>).GetEnumerator()
    interface IEnumerable with member __.GetEnumerator() = (upcast changes : IEnumerable).GetEnumerator()

