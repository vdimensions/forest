namespace Forest
open System
open System.Collections
open System.Collections.Generic;

[<Struct>]
[<CustomComparison>]
[<CustomEquality>]
type ComparisonAdapter<'T>(value: 'T, comparer: IComparer<'T>, eqComparer: IEqualityComparer<'T>) =
    new(value) = ComparisonAdapter(value, Comparer<'T>.Default, EqualityComparer<'T>.Default)
    member this.CompareTo (cmp: IComparer<'T>, v: 'T) = cmp.Compare(v, value)
    member this.CompareTo (v: 'T) = this.CompareTo(comparer, v)
    member this.CompareTo (c: ComparisonAdapter<'T>) = c.CompareTo(comparer, value)
    member this.CompareTo (o: obj) = 
        if (o :? Comparison<'T>) then this.CompareTo(downcast o: ComparisonAdapter<'T>)
        else if (o :? 'T) then this.CompareTo(downcast o: 'T)
        else if (o :? IComparable) then ((downcast o: IComparable)).CompareTo(value)
        else raise (NotSupportedException ())
    member this.Equals (c: ComparisonAdapter<'T>): bool = c.Equals(eqComparer, value)
    member this.Equals (cmp: IEqualityComparer<'T>, v: 'T): bool = cmp.Equals(v, value)
    member this.Equals (v: 'T): bool = eqComparer.Equals(v, value)
    override this.Equals (o: obj): bool =
        if (o :? Comparison<'T>) then this.Equals(downcast o: ComparisonAdapter<'T>)
        else if (o :? 'T) then this.Equals(downcast o: 'T)
        else false
    override this.GetHashCode () = eqComparer.GetHashCode value
    member this.Value with get () = value
    interface IEquatable<'T> with member this.Equals other = this.Equals(eqComparer, other)
    interface IComparable<'T> with member this.CompareTo other = this.CompareTo(comparer, other)
    interface IComparable with member this.CompareTo o = this.CompareTo o

type [<AutoOpen>] IIndex<'T, 'TKey> =
    inherit IEnumerable<'T>
    abstract member Contains: item: 'T -> bool
    abstract member ContainsKey: key: 'TKey -> bool
    abstract Count: int with get
    abstract Keys: IEnumerable<'TKey> with get
    abstract Item: 'TKey -> Option<'T> with get

type [<AutoOpen>] IWriteableIndex<'T, 'TKey> =
    inherit IIndex<'T, 'TKey>
    abstract member Remove: key: 'TKey -> IWriteableIndex<'T, 'TKey>
    abstract member Insert: key: 'TKey -> item: 'T -> IWriteableIndex<'T, 'TKey>
    abstract member Clear: unit -> IWriteableIndex<'T, 'TKey>

type [<AutoOpen>] IAutoIndex<'T, 'TKey> =
    inherit IWriteableIndex<'T, 'TKey>
    abstract member Remove: key: 'TKey -> IAutoIndex<'T, 'TKey> 
    abstract member Remove: item: 'T -> IAutoIndex<'T, 'TKey> 
    abstract member Add: item: 'T -> IAutoIndex<'T, 'TKey> 

[<AbstractClass>]
type AbstractWriteableIndex<'T, 'TKey, 'R  when 'R:> IWriteableIndex<'T, 'TKey >>() as self = 
    abstract member Contains: 'T -> bool
    abstract member ContainsKey: 'TKey -> bool
    abstract member Insert: 'TKey -> 'T -> 'R
    abstract member Remove: 'TKey -> 'R
    abstract member Clear: unit -> 'R
    abstract member GetEnumerator: unit -> IEnumerator<'T>
    abstract Count: int with get
    abstract Keys: IEnumerable<'TKey> with get
    abstract Item: 'TKey -> Option<'T> with get
    interface IWriteableIndex<'T, 'TKey> with
        member x.Insert k v = upcast x.Insert k v : IWriteableIndex<'T, 'TKey>
        member x.Remove k = upcast x.Remove k : IWriteableIndex<'T, 'TKey>
        member x.Clear () = upcast x.Clear () : IWriteableIndex<'T, 'TKey>
    interface IIndex<'T, 'TKey> with
        member x.Contains item = self.Contains item
        member x.ContainsKey k = self.ContainsKey k
        member x.Count = self.Count
        member x.Keys with get () = self.Keys
        member x.Item with get k = self.[k]
    interface IEnumerable<'T> with member x.GetEnumerator () = x.GetEnumerator()
    interface IEnumerable with member x.GetEnumerator () = upcast x.GetEnumerator(): IEnumerator

[<AbstractClass>]
type IndexProxy<'T, 'TKey, 'R when 'R :> IndexProxy<'T, 'TKey, 'R>>(target: IWriteableIndex<'T, 'TKey>, resolve: IWriteableIndex<'T, 'TKey> -> 'R) =
    abstract member Contains: 'T -> bool
    default this.Contains item =  target.Contains item
    abstract member ContainsKey: 'TKey -> bool
    default this.ContainsKey key =  target.ContainsKey key
    abstract member Insert: 'TKey -> 'T -> 'R
    default this.Insert k v =  resolve (target.Insert k v)
    abstract member Remove: 'TKey -> 'R
    default this.Remove k =  resolve (target.Remove k)
    abstract member Clear: unit -> 'R
    default this.Clear () =  resolve (target.Clear ())
    abstract member GetEnumerator: unit -> IEnumerator<'T>
    default this.GetEnumerator() = target.GetEnumerator()
    abstract Count: int with get
    default this.Count with get() = target.Count
    abstract Keys: IEnumerable<'TKey> with get
    default this.Keys with get() = target.Keys
    abstract Item: 'TKey -> Option<'T> with get
    default this.Item with get k = target.[k]
    //member protected this.Target with get () = target
    interface IWriteableIndex<'T, 'TKey> with
        member x.Insert k v = upcast x.Insert k v : IWriteableIndex<'T, 'TKey>
        member x.Remove k = upcast x.Remove k : IWriteableIndex<'T, 'TKey>
        member x.Clear () = upcast x.Clear () : IWriteableIndex<'T, 'TKey>
    interface IIndex<'T, 'TKey> with
        member x.Contains item = x.Contains item
        member x.ContainsKey key = x.ContainsKey key
        member x.Count = x.Count
        member x.Keys with get () = x.Keys
        member x.Item with get k = x.[k]
    interface IEnumerable<'T> with member x.GetEnumerator () = x.GetEnumerator()
    interface IEnumerable with member x.GetEnumerator () = upcast x.GetEnumerator(): IEnumerator
