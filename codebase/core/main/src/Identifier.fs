namespace Forest

open System
open System.Text


type [<Interface>] IForestIdentifier =
    abstract member UniqueID: Guid with get
    abstract member Name: string with get

type RegionID = 
    | Shell 
    | RegionID of Data: ViewID * string * Guid
    member this.Name 
        with get() = 
            match this with 
            | Shell -> String.Empty 
            | RegionID (_, name, _) -> name
    member this.UniqueID 
        with get() =
            match this with
            | Shell -> Guid.Empty
            | RegionID (_, _, guid) -> guid
    override this.ToString() = 
        match this with
        | Shell -> this.Name
        | RegionID (a, b, c) -> StringBuilder(a.ToString()).Append(ViewID.Separator).Append(b).Append(ViewID.IndexSuffix).Append(c.ToString()).ToString()
    interface IForestIdentifier with
        member this.Name with get() = this.Name
        member this.UniqueID with get() = this.UniqueID
 and [<Struct>] ViewID(rid: RegionID, name: string, uniqueID: Guid) =
    static member Separator: char = '/'
    static member IndexSuffix: char = '#'

    member __.RegionID 
        with get() = rid
    member __.Name 
        with get() = name
    member __.UniqueID 
        with get() = uniqueID
    override __.ToString() = 
        StringBuilder(rid.ToString()).Append(ViewID.Separator).Append(name).Append(ViewID.IndexSuffix).Append(uniqueID.ToString()).ToString()
    interface IForestIdentifier with
        member this.Name with get() = this.Name
        member this.UniqueID with get() = this.UniqueID

[<CustomComparison>]
[<CustomEquality>]
type Identifier = 
    | View of Value: ViewID 
    | Region of Value: RegionID
    member private this._compareTo (other:Identifier) =
        let inline getGuid id =
            match id with
            | View vid -> vid.UniqueID
            | Region rid -> rid.UniqueID
        let (id1, id2) = (getGuid this, getGuid other)
        id1.CompareTo id2
    member private this._equals other =
        let inline getGuid id =
            match id with
            | View vid -> vid.UniqueID
            | Region rid -> rid.UniqueID
        (getGuid this) = (getGuid other)
    override this.Equals o = 
        match null2opt o with
        | Some v ->
            match v with
            | :? Identifier as other -> this._equals other
            | _ -> false
        | None -> false
    override this.GetHashCode() = (match this with | View vid -> vid.UniqueID | Region rid -> rid.UniqueID).GetHashCode()
    interface IComparable<Identifier> with member this.CompareTo(other:Identifier) = this._compareTo other
    interface IComparable with
        member this.CompareTo o = 
            match o with
            | :? Identifier as id -> this._compareTo id
            | :? IComparable as c -> (c.CompareTo this)*(-1)
            | _ -> raise (NotSupportedException ())
    interface IEquatable<Identifier> with member this.Equals other = this._equals other
    interface IForestIdentifier with
        member this.Name with get() = match this with View v -> v.Name | Region r -> r.Name
        member this.UniqueID with get() = match this with View v -> v.UniqueID | Region r -> r.UniqueID

module Identifier =
    let view id = 
        match id with 
        | View v -> Some (v.Name, v.UniqueID)
        | Region _ -> None 

    let region id = 
        match id with 
        | Region r -> Some (r.Name, r.UniqueID)
        | View _ -> None

    [<CompiledName("IsView")>]
    let isView id =
        match id with
        | View _ -> true
        | _ -> false

    [<CompiledName("IsRegion")>]
    let isRegion id =
        match id with
        | Region _ -> true
        | _ -> false

    [<CompiledName("NameOf")>]
    let nameof id = 
        match id with 
        | View v -> v.Name 
        | Region r -> r.Name
    
    [<CompiledName("IDOf")>]
    let idOf id = 
        match id with 
        | View v -> v.UniqueID 
        | Region r -> r.UniqueID

    let parentOf id =
        match id with
        | Region r ->
            match r with
            | RegionID (viewID, _, _) -> View (viewID) |> Some
            | Shell -> None
        | View v -> Region (v.RegionID) |> Some

    [<CompiledName("Add")>]
    let add id name parent =
        match parent with
        | View v -> Region(RegionID(v, name, id))
        | Region r -> View(ViewID(r, name, id))

    [<CompiledName("Add")>]
    let addNew name parent = add (Guid.NewGuid()) name parent

    [<CompiledName("Shell")>]
    let shell = Identifier.Region(RegionID.Shell)
