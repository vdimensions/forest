namespace Forest

open System

type [<Interface>] IForestIdentifier =
    abstract member UniqueID: Guid with get
    abstract member Name: string with get

//[<CustomComparison>]
//[<CustomEquality>]
type [<Sealed>] Identifier =
    override Equals: obj -> bool
    override GetHashCode: unit -> int
    interface IForestIdentifier
    interface IComparable<Identifier>
    interface IComparable
    interface IEquatable<Identifier>

module Identifier =

    val view: Identifier -> (string*Guid) option

    val region: Identifier -> (string*Guid) option

    [<CompiledName("IsView")>]
    val isView: Identifier -> bool

    [<CompiledName("IsRegion")>]
    val isRegion: Identifier -> bool

    [<CompiledName("NameOf")>]
    val nameof: Identifier -> string
    
    [<CompiledName("IDOf")>]
    val idOf: id:Identifier -> Guid

    val parentOf: id:Identifier -> Identifier option

    [<CompiledName("Add")>]
    val add: Guid -> string -> Identifier -> Identifier

    [<CompiledName("Add")>]
    val addNew: string -> Identifier -> Identifier

    [<CompiledName("Shell")>]
    val shell: Identifier
