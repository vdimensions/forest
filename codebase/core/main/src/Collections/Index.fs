namespace Forest.Collections

open System.Collections
open System.Collections.Generic;


[<AutoOpen>]
type [<Sealed>] Index<'T, 'TKey> internal(dict:IDictionary<'TKey, 'T>) =
    member __.Contains item = dict.Values.Contains item
    member __.ContainsKey key = dict.ContainsKey key
    member __.TryFind key =
        match dict.TryGetValue key with
        | (true, v) -> Some v
        | (false, _) -> None
    member __.Count with get() = dict.Count
    member __.Keys with get() = upcast dict.Keys:IEnumerable<'TKey>
    member __.Item with get(key) = dict.[key]
    interface IEnumerable<'T> with member __.GetEnumerator() = dict.Values.GetEnumerator()
    interface IEnumerable with member __.GetEnumerator() = upcast dict.Values.GetEnumerator():IEnumerator

//[<AutoOpen>]
//type [<Interface>] IIndex<'T, 'TKey> =
//    inherit IEnumerable<'T>
//    abstract member Contains: item: 'T -> bool
//    abstract member ContainsKey: key: 'TKey -> bool
//    abstract member TryFind: key: 'TKey -> Option<'T>
//    abstract Count: int with get
//    abstract Keys: IEnumerable<'TKey> with get
//    abstract Item: 'TKey -> 'T with get
//
//[<AutoOpen>]
//type [<Interface>] IWriteableIndex<'T, 'TKey> =
//    inherit IIndex<'T, 'TKey>
//    abstract member Clear: unit -> IWriteableIndex<'T, 'TKey>
//    abstract member Insert: key: 'TKey -> item: 'T -> IWriteableIndex<'T, 'TKey>
//    abstract member Remove: key: 'TKey -> IWriteableIndex<'T, 'TKey>
//
//[<AutoOpen>]
//type [<Interface>] IAutoIndex<'T, 'TKey> =
//    inherit IWriteableIndex<'T, 'TKey>
//    abstract member Add: item: 'T -> IAutoIndex<'T, 'TKey> 
//    abstract member Remove: key: 'TKey -> IAutoIndex<'T, 'TKey> 
//    abstract member Remove: item: 'T -> IAutoIndex<'T, 'TKey> 
//
//type [<AbstractClass>] AbstractWriteableIndex<'T, 'TKey, 'R  when 'R:> IWriteableIndex<'T, 'TKey >>() as self = 
//    abstract member Contains: 'T -> bool
//    abstract member ContainsKey: 'TKey -> bool
//    abstract member TryFind: 'TKey -> Option<'T>
//    abstract member Insert: 'TKey -> 'T -> 'R
//    abstract member Remove: 'TKey -> 'R
//    abstract member Clear: unit -> 'R
//    abstract member GetEnumerator: unit -> IEnumerator<'T>
//    abstract Count: int with get
//    abstract Keys: IEnumerable<'TKey> with get
//    abstract Item: 'TKey -> 'T with get
//    interface IWriteableIndex<'T, 'TKey> with
//        member __.Insert k v = upcast self.Insert k v : IWriteableIndex<'T, 'TKey>
//        member __.Remove k = upcast self.Remove k : IWriteableIndex<'T, 'TKey>
//        member __.Clear () = upcast self.Clear () : IWriteableIndex<'T, 'TKey>
//    interface IIndex<'T, 'TKey> with
//        member __.Contains item = self.Contains item
//        member __.ContainsKey k = self.ContainsKey k
//        member __.TryFind k = self.TryFind k
//        member __.Count = self.Count
//        member __.Keys with get () = self.Keys
//        member __.Item with get k = self.[k]
//    interface IEnumerable<'T> with member __.GetEnumerator () = self.GetEnumerator()
//    interface IEnumerable with member __.GetEnumerator () = upcast self.GetEnumerator(): IEnumerator
//
//type [<AbstractClass>] IndexProxy<'T, 'TKey, 'R when 'R :> IndexProxy<'T, 'TKey, 'R>>(target: IWriteableIndex<'T, 'TKey>, resolve: IWriteableIndex<'T, 'TKey> -> 'R) as self =
//    abstract member Contains: 'T -> bool
//    default __.Contains item = target.Contains item
//    abstract member ContainsKey: 'TKey -> bool
//    default __.ContainsKey key = target.ContainsKey key
//    abstract member TryFind: 'TKey -> Option<'T>
//    default __.TryFind key = target.TryFind key
//    abstract member Insert: 'TKey -> 'T -> 'R
//    default __.Insert k v = resolve (target.Insert k v)
//    abstract member Remove: 'TKey -> 'R
//    default __.Remove k = resolve (target.Remove k)
//    abstract member Clear: unit -> 'R
//    default __.Clear () = resolve (target.Clear ())
//    abstract member GetEnumerator: unit -> IEnumerator<'T>
//    default __.GetEnumerator() = target.GetEnumerator()
//    abstract Count: int with get
//    default __.Count with get() = target.Count
//    abstract Keys: IEnumerable<'TKey> with get
//    default __.Keys with get() = target.Keys
//    abstract Item: 'TKey -> 'T with get
//    default __.Item with get k = target.[k]
//    interface IWriteableIndex<'T, 'TKey> with
//        member __.Insert k v = upcast self.Insert k v : IWriteableIndex<'T, 'TKey>
//        member __.Remove k = upcast self.Remove k : IWriteableIndex<'T, 'TKey>
//        member __.Clear () = upcast self.Clear () : IWriteableIndex<'T, 'TKey>
//    interface IIndex<'T, 'TKey> with
//        member __.Contains item = self.Contains item
//        member __.ContainsKey key = self.ContainsKey key
//        member __.TryFind key = self.TryFind key
//        member __.Count = self.Count
//        member __.Keys with get () = self.Keys
//        member __.Item with get k = self.[k]
//    interface IEnumerable<'T> with member __.GetEnumerator () = self.GetEnumerator()
//    interface IEnumerable with member __.GetEnumerator () = upcast self.GetEnumerator(): IEnumerator
