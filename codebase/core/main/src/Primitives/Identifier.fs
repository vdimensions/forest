namespace Forest

open System


[<CustomComparison>]
[<CustomEquality>]
type Identifier = 
    | Shell_
    | ViewID_ of Identifier*string*string*Guid
    [<CompiledName("Shell")>]
    static member shell = Identifier.Shell_
    member this.Parent with get() = match this with Shell_ -> Shell_ | ViewID_ (p, _, _, _) -> p
    member this.Region with get() = match this with Shell_ -> String.Empty | ViewID_ (_, r, _, _) -> r
    member this.Name with get() = match this with Shell_ -> String.Empty | ViewID_ (_, _, v, _) -> v
    member this.UniqueID with get() = match this with Shell_ -> Guid.Empty | ViewID_ (_, _, _, g) -> g
    member this.Fragment with get() = match this with Shell_ -> "/" | ViewID_ (_, r, v, _) -> String.Format("{0}#{1}/", r, v)
    member private this._compareTo (other:Identifier) = StringComparer.Ordinal.Compare(this.Fragment, other.Fragment)
    member private this._equals (other: Identifier) = this.UniqueID = other.UniqueID
    override this.Equals o = 
        match null2opt o with
        | Some v ->
            match v with
            | :? Identifier as other -> this._equals other
            | _ -> false
        | None -> false
    override this.GetHashCode() = this.UniqueID.GetHashCode()
    interface IComparable<Identifier> with member this.CompareTo other = this._compareTo other
    interface IComparable with
        member this.CompareTo o = 
            match o with
            | :? Identifier as id -> this._compareTo id
            | :? IComparable as c -> (-1)*(c.CompareTo this)
            | _ -> raise (NotSupportedException ())
    interface IEquatable<Identifier> with member this.Equals other = this._equals other

module internal Identifier =
    let add id region name parent = Identifier.ViewID_(parent, region, name, id)

    let addNew region name parent = add (Guid.NewGuid()) region name parent

    let isShell (id:Identifier) = match id with Identifier.Shell_ -> true | _ -> false
    
