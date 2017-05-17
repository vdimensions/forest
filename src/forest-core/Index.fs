namespace Forest
open System.Collections
open System.Collections.Generic;


type [<AutoOpen>] IIndex<'T, 'TKey> =
    inherit IEnumerable<'T>
    abstract Count          : int with get
    abstract Keys           : IEnumerable<'TKey> with get
    abstract Item           : 'TKey -> 'T with get

type [<AutoOpen>] IWriteableIndex<'T, 'TKey> =
    inherit IIndex<'T, 'TKey>
    abstract member Remove  : key: 'TKey -> IWriteableIndex<'T, 'TKey>
    abstract member Insert  : key: 'TKey -> item: 'T -> IWriteableIndex<'T, 'TKey>
    abstract member Clear   : unit -> IWriteableIndex<'T, 'TKey>

type [<AutoOpen>] IAutoIndex<'T, 'TKey> =
    inherit IWriteableIndex<'T, 'TKey>
    abstract member Remove: item: 'T -> IAutoIndex<'T, 'TKey> 
    abstract member Add: item: 'T -> IAutoIndex<'T, 'TKey> 

[<AbstractClass>]
type AbstractWriteableIndex<'T, 'TKey, 'R  when 'R:> IWriteableIndex<'T, 'TKey >>() = 
    abstract member Insert: 'TKey -> 'T -> 'R
    abstract member Remove: 'TKey -> 'R
    abstract member Clear: unit -> 'R
    abstract member GetEnumerator: unit -> IEnumerator<'T>
    abstract Count: int with get
    abstract Keys: IEnumerable<'TKey> with get
    abstract Item: 'TKey -> 'T with get
    interface IWriteableIndex<'T, 'TKey> with
        member x.Insert k v = upcast x.Insert k v : IWriteableIndex<'T, 'TKey>
        member x.Remove k = upcast x.Remove k : IWriteableIndex<'T, 'TKey>
        member x.Clear () = upcast x.Clear () : IWriteableIndex<'T, 'TKey>
    interface IIndex<'T, 'TKey> with
        member x.Count = x.Count
        member x.Keys with get () = x.Keys
        member x.Item with get k = x.[k]
    interface IEnumerable<'T> with member x.GetEnumerator () = x.GetEnumerator()
    interface IEnumerable with member x.GetEnumerator () = upcast x.GetEnumerator(): IEnumerator

[<AbstractClass>]
type IndexProxy<'T, 'TKey, 'R when 'R :> IndexProxy<'T, 'TKey, 'R>>(target: IWriteableIndex<'T, 'TKey>) =
    abstract member Resolve: IWriteableIndex<'T, 'TKey>  -> 'R
    abstract member Insert: 'TKey -> 'T -> 'R
    default this.Insert k v =  this.Resolve (target.Insert k v)
    abstract member Remove: 'TKey -> 'R
    default this.Remove k =  this.Resolve (target.Remove k)
    abstract member Clear: unit -> 'R
    default this.Clear () =  this.Resolve (target.Clear ())
    abstract member GetEnumerator: unit -> IEnumerator<'T>
    default this.GetEnumerator() = target.GetEnumerator()
    abstract Count: int with get
    default this.Count with get() = target.Count
    abstract Keys: IEnumerable<'TKey> with get
    default this.Keys with get() = target.Keys
    abstract Item: 'TKey -> 'T with get
    default this.Item with get k = target.[k]
    //member protected this.Target with get () = target
    interface IWriteableIndex<'T, 'TKey> with
        member x.Insert k v = upcast x.Insert k v : IWriteableIndex<'T, 'TKey>
        member x.Remove k = upcast x.Remove k : IWriteableIndex<'T, 'TKey>
        member x.Clear () = upcast x.Clear () : IWriteableIndex<'T, 'TKey>
    interface IIndex<'T, 'TKey> with
        member x.Count = x.Count
        member x.Keys with get () = x.Keys
        member x.Item with get k = x.[k]
    interface IEnumerable<'T> with member x.GetEnumerator () = x.GetEnumerator()
    interface IEnumerable with member x.GetEnumerator () = upcast x.GetEnumerator(): IEnumerator
