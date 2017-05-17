namespace Forest
open System.Collections
open System.Collections.Generic

[<Sealed>]
type WriteableIndex<'T, 'TKey when 'TKey: comparison>(map : Map<'TKey, 'T>) = class
    inherit AbstractWriteableIndex<'T, 'TKey, WriteableIndex<'T, 'TKey> >()
    new() = WriteableIndex(Map.empty)
    override this.Insert k v = new WriteableIndex<'T, 'TKey>(map.Add(k,v))
    override this.Remove k = new WriteableIndex<'T, 'TKey>(map.Remove(k))
    override this.Clear () = new WriteableIndex<'T, 'TKey>()
    override this.GetEnumerator () = (upcast map: IDictionary<'TKey, 'T>).Values.GetEnumerator()
    override this.Count = map.Count
    override this.Keys with get () = upcast (upcast map: IDictionary<'TKey, 'T>).Keys : IEnumerable<'TKey>
    override this.Item with get k = map.[k]
end

[<Sealed>]
type AutoIndex<'T, 'TKey when 'TKey: comparison>(keyFn: 'T -> 'TKey, ix: IWriteableIndex<'T, 'TKey>) =
    inherit IndexProxy<'T, 'TKey, AutoIndex<'T, 'TKey>>(ix)
    new(keyFn: 'T -> 'TKey) = AutoIndex(keyFn, new WriteableIndex<'T, 'TKey>())
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