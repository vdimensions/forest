namespace Forest

open System
open System.Text


[<CustomComparison>]
[<CustomEquality>]
type HierarchyKey = 
    | Shell_
    | ViewID_ of parent:HierarchyKey * region:string * view:string * hash:string
    [<CompiledName("Shell")>]
    static member shell = HierarchyKey.Shell_
    member this.Parent with get() = match this with Shell_ -> Shell_ | ViewID_ (p, _, _, _) -> p
    member this.Region with get() = match this with Shell_ -> String.Empty | ViewID_ (_, r, _, _) -> r
    member this.View with get() = match this with Shell_ -> String.Empty | ViewID_ (_, _, v, _) -> v
    member this.Hash with get() = match this with Shell_ -> ForestID.empty.Hash | ViewID_ (_, _, _, h) -> h
    member private this._compareTo (other:HierarchyKey) = StringComparer.Ordinal.Compare(this.Hash, other.Hash)
    member private this._equals (other: HierarchyKey) = StringComparer.Ordinal.Equals(this.Hash, other.Hash)
    override this.Equals o = 
        match null2opt o with
        | Some v ->
            match v with
            | :? HierarchyKey as other -> this._equals other
            | _ -> false
        | None -> false
    override this.GetHashCode() = this.Hash.GetHashCode()
    member private this.ToStringBuilder() =
        match this with 
        | Shell_ -> StringBuilder("/")
        | ViewID_ (p, r, v, h) -> p.ToStringBuilder().AppendFormat("{0}>{1}#{2}", (if r.Length = 0 then "shell" else r), v, h).Append('/')
    override this.ToString() = this.ToStringBuilder().ToString()
    interface IComparable<HierarchyKey> with member this.CompareTo other = this._compareTo other
    interface IComparable with
        member this.CompareTo o = 
            match o with
            | :? HierarchyKey as id -> this._compareTo id
            | :? IComparable as c -> (-1)*(c.CompareTo this)
            | _ -> raise (NotSupportedException ())
    interface IEquatable<HierarchyKey> with member this.Equals other = this._equals other

module internal HierarchyKey =
    let add id region name parent = HierarchyKey.ViewID_(parent, region, name, id)

    let addNew region name parent = add (ForestID.newID().Hash) region name parent

    let isShell (id:HierarchyKey) = match id with HierarchyKey.Shell_ -> true | _ -> false
    
