﻿namespace Forest

open System.Collections.Generic


type [<Sealed>] WriteableIndex<'T, 'TKey>(map : Map<ComparisonAdapter<'TKey>, 'T>, eqComparer : IEqualityComparer<'TKey>, comparer: IComparer<'TKey>) =
    inherit AbstractWriteableIndex<'T, 'TKey, WriteableIndex<'T, 'TKey> >()
    new(map : Map<ComparisonAdapter<'TKey>, 'T>, eqComparer: IEqualityComparer<'TKey>) = WriteableIndex(map, eqComparer, Comparer<'TKey>.Default)
    new(map : Map<ComparisonAdapter<'TKey>, 'T>) = WriteableIndex(map, EqualityComparer<'TKey>.Default)
    new(eqComparer : IEqualityComparer<'TKey>, comparer: IComparer<'TKey>) = WriteableIndex(Map.empty, eqComparer, comparer)
    new() = WriteableIndex(Map.empty, EqualityComparer<'TKey>.Default, Comparer<'TKey>.Default)
    override __.Contains item = (upcast map: IDictionary<ComparisonAdapter<'TKey>, 'T>).Values.Contains item
    override __.ContainsKey key = map.ContainsKey (new ComparisonAdapter<'TKey>(key, comparer, eqComparer))
    override __.TryFind key = map.TryFind (new ComparisonAdapter<'TKey>(key, comparer, eqComparer))
    override __.Insert k v = new WriteableIndex<'T, 'TKey>(map.Add(new ComparisonAdapter<'TKey>(k, comparer, eqComparer), v), eqComparer, comparer)
    override __.Remove k = new WriteableIndex<'T, 'TKey>(map.Remove(new ComparisonAdapter<'TKey>(k, comparer, eqComparer)), eqComparer, comparer)
    override __.Clear () = new WriteableIndex<'T, 'TKey>(eqComparer, comparer)
    override __.GetEnumerator () = (upcast map: IDictionary<ComparisonAdapter<'TKey>, 'T>).Values.GetEnumerator()
    override __.Count = map.Count
    override __.Keys = 
        let keys = (upcast map: IDictionary<ComparisonAdapter<'TKey>, 'T>).Keys
        let inline keySelector (k: ComparisonAdapter<'TKey>) = k.Value
        keys |> Seq.map keySelector
    override __.Item with get k = map.[new ComparisonAdapter<'TKey>(k, comparer, eqComparer)]

type [<Sealed>] AutoIndex<'T, 'TKey>(keyFn: 'T -> 'TKey, ix: IWriteableIndex<'T, 'TKey>) =
    inherit IndexProxy<'T, 'TKey, AutoIndex<'T, 'TKey>>(ix, (fun x -> new AutoIndex<'T, 'TKey>(keyFn, x)))
    new(keyFn: 'T -> 'TKey) = AutoIndex(keyFn, new WriteableIndex<'T, 'TKey>())
    member __.Remove item = 
        let key: 'TKey = keyFn item
        new AutoIndex<'T, 'TKey>(keyFn, (ix.Remove key))
    member __.Add item = 
        let key: 'TKey = keyFn item
        new AutoIndex<'T, 'TKey>(keyFn, (ix.Insert key item))
    interface IAutoIndex<'T, 'TKey> with
        member __.Add item = 
            let key: 'TKey = keyFn item
            upcast new AutoIndex<'T, 'TKey>(keyFn, (ix.Insert key item)) : IAutoIndex<'T, 'TKey>
        member __.Remove item = 
            let key: 'TKey = keyFn item
            upcast new AutoIndex<'T, 'TKey>(keyFn, (ix.Remove key )) : IAutoIndex<'T, 'TKey>
        member __.Remove (key: 'TKey) = upcast new AutoIndex<'T, 'TKey>(keyFn, (ix.Remove key)) : IAutoIndex<'T, 'TKey>