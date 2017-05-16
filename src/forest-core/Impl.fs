namespace Forest
open System


type MutableIndex<'T, 'TKey when 'TKey: comparison>(map : Map<'TKey, 'T>) = 
    new() = MutableIndex(Map.empty)
    interface IMutableIndex<'T, 'TKey> with
        member x.Insert k v =
            upcast new MutableIndex<'T, 'TKey>(map.Add(k,v)) : IMutableIndex<'T, 'TKey>
        member x.Remove k =
            upcast new MutableIndex<'T, 'TKey>(map.Remove(k)) : IMutableIndex<'T, 'TKey>
        member x.Clear () =
            upcast new MutableIndex<'T, 'TKey>() : IMutableIndex<'T, 'TKey>
