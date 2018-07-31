namespace Forest
open System.Collections.Generic
open System.Linq


type [<Sealed>] WriteableIndex<'T, 'TKey>(map : Map<ComparisonAdapter<'TKey>, 'T>, eqComparer : IEqualityComparer<'TKey>, comparer: IComparer<'TKey>) =
    inherit AbstractWriteableIndex<'T, 'TKey, WriteableIndex<'T, 'TKey> >()
    new(map : Map<ComparisonAdapter<'TKey>, 'T>, eqComparer: IEqualityComparer<'TKey>) = WriteableIndex(map, eqComparer, Comparer<'TKey>.Default)
    new(map : Map<ComparisonAdapter<'TKey>, 'T>) = WriteableIndex(map, EqualityComparer<'TKey>.Default)
    new(eqComparer : IEqualityComparer<'TKey>, comparer: IComparer<'TKey>) = WriteableIndex(Map.empty, eqComparer, comparer)
    new() = WriteableIndex(Map.empty, EqualityComparer<'TKey>.Default, Comparer<'TKey>.Default)
    override this.Contains item = (upcast map: IDictionary<ComparisonAdapter<'TKey>, 'T>).Values.Contains item
    override this.ContainsKey key = map.ContainsKey (new ComparisonAdapter<'TKey>(key, comparer, eqComparer))
    override this.Insert k v = new WriteableIndex<'T, 'TKey>(map.Add(new ComparisonAdapter<'TKey>(k, comparer, eqComparer), v), eqComparer, comparer)
    override this.Remove k = new WriteableIndex<'T, 'TKey>(map.Remove(new ComparisonAdapter<'TKey>(k, comparer, eqComparer)), eqComparer, comparer)
    override this.Clear () = new WriteableIndex<'T, 'TKey>(eqComparer, comparer)
    override this.GetEnumerator () = (upcast map: IDictionary<ComparisonAdapter<'TKey>, 'T>).Values.GetEnumerator()
    override this.Count = map.Count
    override this.Keys = 
        let keys = (upcast map: IDictionary<ComparisonAdapter<'TKey>, 'T>).Keys
        let keySelector = (fun (k: ComparisonAdapter<'TKey>) -> k.Value)
        keys.Select(keySelector)
    override this.Item 
        with get k = 
            let key = new ComparisonAdapter<'TKey>(k, comparer, eqComparer)
            if (map.ContainsKey(key)) then null2opt map.[key]
            else None


type [<Sealed>] AutoIndex<'T, 'TKey>(keyFn: 'T -> 'TKey, ix: IWriteableIndex<'T, 'TKey>) =
    inherit IndexProxy<'T, 'TKey, AutoIndex<'T, 'TKey>>(ix, (fun x -> new AutoIndex<'T, 'TKey>(keyFn, x)))
    //new(keyFn: 'T -> 'TKey) = AutoIndex(keyFn, new WriteableIndex<'T, 'TKey>())
    member this.Remove item = 
        let key: 'TKey = keyFn item
        new AutoIndex<'T, 'TKey>(keyFn, (ix.Remove key))
    member this.Add item = 
        let key: 'TKey = keyFn item
        new AutoIndex<'T, 'TKey>(keyFn, (ix.Insert key item))
    interface IAutoIndex<'T, 'TKey> with
        member x.Add item = 
            let key: 'TKey = keyFn item
            upcast new AutoIndex<'T, 'TKey>(keyFn, (ix.Insert key item)) : IAutoIndex<'T, 'TKey>
        member x.Remove item = 
            let key: 'TKey = keyFn item
            upcast new AutoIndex<'T, 'TKey>(keyFn, (ix.Remove key )) : IAutoIndex<'T, 'TKey>
        member x.Remove (key: 'TKey) = upcast new AutoIndex<'T, 'TKey>(keyFn, (ix.Remove key)) : IAutoIndex<'T, 'TKey>