namespace Forest
open System
open System.Collections
open System.Collections.Generic
open System.Linq

[<Sealed>]
type WriteableIndex<'T, 'TKey>(map : Map<ComparisonAdapter<'TKey>, 'T>, eqComparer : IEqualityComparer<'TKey>, comparer: IComparer<'TKey>) = class
    inherit AbstractWriteableIndex<'T, 'TKey, WriteableIndex<'T, 'TKey> >()
    new(map : Map<ComparisonAdapter<'TKey>, 'T>, eqComparer: IEqualityComparer<'TKey>) = WriteableIndex(map, eqComparer, Comparer<'TKey>.Default)
    new(map : Map<ComparisonAdapter<'TKey>, 'T>) = WriteableIndex(map, EqualityComparer<'TKey>.Default)
    new(map : Map<ComparisonAdapter<'TKey>, 'T>) = WriteableIndex(map, EqualityComparer<'TKey>.Default)
    new(eqComparer : IEqualityComparer<'TKey>, comparer: IComparer<'TKey>) = WriteableIndex(Map.empty, eqComparer, comparer)
    new() = WriteableIndex(Map.empty, EqualityComparer<'TKey>.Default, Comparer<'TKey>.Default)
    // TODO: support comparer
    //new(comparer: IEqualityComparer<'TKey>) = WriteableIndex(new Map<'TKey, 'T, comparer>())
    override this.Contains item = (upcast map: IDictionary<ComparisonAdapter<'TKey>, 'T>).Values.Contains item
    override this.ContainsKey key = 
        let actualKey = new ComparisonAdapter<'TKey>(key, comparer, eqComparer)
        map.ContainsKey actualKey
    override this.Insert k v = 
        let actualKey = new ComparisonAdapter<'TKey>(k, comparer, eqComparer)
        new WriteableIndex<'T, 'TKey>(map.Add(actualKey,v), eqComparer, comparer)
    override this.Remove k = 
        let actualKey = new ComparisonAdapter<'TKey>(k, comparer, eqComparer)
        new WriteableIndex<'T, 'TKey>(map.Remove(actualKey), eqComparer, comparer)
    override this.Clear () = new WriteableIndex<'T, 'TKey>(eqComparer, comparer)

    override this.GetEnumerator () = (upcast map: IDictionary<ComparisonAdapter<'TKey>, 'T>).Values.GetEnumerator()
    override this.Count = map.Count
    override this.Keys with get () = 
        let keys = (upcast map: IDictionary<ComparisonAdapter<'TKey>, 'T>).Keys
        let keySelector = fun (k) -> k.Value
        keys.Select(keySelector)
    override this.Item with get k = 
        let actualKey = new ComparisonAdapter<'TKey>(k, comparer, eqComparer)
        map.[actualKey]
end

[<Sealed>]
type AutoIndex<'T, 'TKey>(keyFn: 'T -> 'TKey, ix: IWriteableIndex<'T, 'TKey>) =
    inherit IndexProxy<'T, 'TKey, AutoIndex<'T, 'TKey>>(ix)
    //new(keyFn: 'T -> 'TKey) = AutoIndex(keyFn, new WriteableIndex<'T, 'TKey>())
    override this.Resolve x = new AutoIndex<'T, 'TKey>(keyFn, x)
    member this.Remove item = 
        let key: 'TKey = keyFn item
        let res = ix.Remove key
        this.Resolve res
    member this.Add item = 
        let key: 'TKey = keyFn item
        let res = ix.Insert key item 
        this.Resolve res
    interface IAutoIndex<'T, 'TKey> with
        member x.Add item = 
            let key: 'TKey = keyFn item
            let res = ix.Insert key item 
            upcast x.Resolve res : IAutoIndex<'T, 'TKey>
        member x.Remove item = 
            let key: 'TKey = keyFn item
            let res = ix.Remove key 
            upcast x.Resolve res : IAutoIndex<'T, 'TKey>
        member x.Remove (key: 'TKey) = 
            let res = ix.Renove key
            upcast x.Resolve res : IAutoIndex<'T, 'TKey>