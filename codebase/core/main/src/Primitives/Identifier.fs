namespace Forest

open System
open System.Text


type [<Interface>] IForestIdentifier =
    abstract member Name: string with get
    abstract member UniqueID: Guid with get

[<CustomComparison>]
[<CustomEquality>]
type Identifier = 
    | Shell
    | ViewID of Identifier*string*string*Guid
    member this.Parent with get() = match this with Shell -> Shell | ViewID (p, _, _, _) -> p
    member this.Region with get() = match this with Shell -> String.Empty | ViewID (_, r, _, _) -> r
    member this.Name with get() = match this with Shell -> String.Empty | ViewID (_, _, v, _) -> v
    member this.UniqueID with get() = match this with Shell -> Guid.Empty | ViewID (_, _, _, g) -> g
    member this.Fragment with get() = match this with Shell -> "/" | ViewID (_, r, v, _) -> String.Format("{0}#{1}/", r, v)
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
    interface IForestIdentifier with
        member this.Name with get() = this.Name
        member this.UniqueID with get() = this.UniqueID

module Identifier =
    let internal add id region name parent = Identifier.ViewID(parent, region, name, id)

    let internal addNew region name parent = add (Guid.NewGuid()) region name parent

    let internal isShell (id:Identifier) = match id with Identifier.Shell -> true | _ -> false

    [<CompiledName("Shell")>]
    let shell = Identifier.Shell
