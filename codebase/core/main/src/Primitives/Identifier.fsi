namespace Forest

open System

type [<Interface>] IForestIdentifier =
    abstract member Name: string with get
    abstract member UniqueID: Guid with get

type [<Sealed>] Identifier =
    member Parent: Identifier with get
    member Region: string with get
    member Name: string with get
    member UniqueID: Guid with get
    member Fragment: string with get
    override Equals: obj -> bool
    override GetHashCode: unit -> int
    interface IForestIdentifier
    interface IComparable<Identifier>
    interface IComparable
    interface IEquatable<Identifier>

module Identifier =
    val internal add: Guid -> string -> string -> Identifier -> Identifier

    val internal addNew: string -> string -> Identifier -> Identifier

    val internal isShell: Identifier -> bool

    [<CompiledName("Shell")>]
    val shell: Identifier
