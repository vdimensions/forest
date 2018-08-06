namespace Forest
open System
open System.Collections.Generic

[<CustomComparison>]
[<CustomEquality>]
type [<Struct>] ComparisonAdapter<'T>(value: 'T, comparer: IComparer<'T>, eqComparer: IEqualityComparer<'T>) =
    new(value) = ComparisonAdapter(value, Comparer<'T>.Default, EqualityComparer<'T>.Default)
    member __.CompareTo (cmp: IComparer<'T>, v: 'T) = cmp.Compare(v, value)
    member this.CompareTo (v: 'T) = this.CompareTo(comparer, v)
    member __.CompareTo (c: ComparisonAdapter<'T>) = c.CompareTo(comparer, value)
    member this.CompareTo (o: obj) = 
        if (o :? ComparisonAdapter<'T>) then this.CompareTo(downcast o: ComparisonAdapter<'T>)
        else if (o :? 'T) then this.CompareTo(downcast o: 'T)
        else if (o :? IComparable) then ((downcast o: IComparable)).CompareTo(value)
        else raise (NotSupportedException ())
    member __.Equals (c: ComparisonAdapter<'T>): bool = c.Equals(eqComparer, value)
    member __.Equals (cmp: IEqualityComparer<'T>, v: 'T): bool = cmp.Equals(v, value)
    member __.Equals (v: 'T): bool = eqComparer.Equals(v, value)
    override this.Equals (o: obj): bool =
        if (o :? ComparisonAdapter<'T>) then this.Equals(downcast o: ComparisonAdapter<'T>)
        else if (o :? 'T) then this.Equals(downcast o: 'T)
        else false
    override __.GetHashCode () = eqComparer.GetHashCode value
    member __.Value with get () = value
    interface IEquatable<'T> with member this.Equals other = this.Equals(eqComparer, other)
    interface IComparable<'T> with member this.CompareTo other = this.CompareTo(comparer, other)
    interface IComparable with member this.CompareTo o = this.CompareTo o
