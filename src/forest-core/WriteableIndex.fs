namespace Forest
open System.Collections
open System.Collections.Generic


type WriteableIndex<'T, 'TKey when 'TKey: comparison>(map : Map<'TKey, 'T>) = 
    new() = WriteableIndex(Map.empty)
    interface IWritableIndex<'T, 'TKey> with
        member x.Insert k v = upcast new WriteableIndex<'T, 'TKey>(map.Add(k,v)) : IWritableIndex<'T, 'TKey>
        member x.Remove k = upcast new WriteableIndex<'T, 'TKey>(map.Remove(k)) : IWritableIndex<'T, 'TKey>
        member x.Clear () = upcast new WriteableIndex<'T, 'TKey>() : IWritableIndex<'T, 'TKey>
    interface IIndex<'T, 'TKey> with
        member x.Count = map.Count
        member x.Keys with get () = upcast (upcast map: IDictionary<'TKey, 'T>).Keys : IEnumerable<'TKey>
        member x.Item with get k = map.[k]
    interface IEnumerable<'T> with
        member x.GetEnumerator () = (upcast map: IDictionary<'TKey, 'T>).Values.GetEnumerator()
    interface IEnumerable with
        member x.GetEnumerator () = upcast (upcast map: IDictionary<'TKey, 'T>).Values.GetEnumerator() : IEnumerator
        