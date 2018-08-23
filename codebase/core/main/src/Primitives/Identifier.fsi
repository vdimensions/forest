namespace Forest

open System

type [<Sealed>] Identifier =
    [<CompiledName("Shell")>]
    static member shell: Identifier
    member Parent: Identifier with get
    member Region: string with get
    member View: string with get
    member UniqueID: ForestID with get
    member Fragment: string with get
    override Equals: obj -> bool
    override GetHashCode: unit -> int
    interface IComparable<Identifier>
    interface IComparable
    interface IEquatable<Identifier>

module internal Identifier =
    val add: ForestID -> string -> string -> Identifier -> Identifier

    val addNew: string -> string -> Identifier -> Identifier

    val isShell: Identifier -> bool
