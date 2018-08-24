namespace Forest

open System

type [<Sealed>] HierarchyKey =
    [<CompiledName("Shell")>]
    static member shell: HierarchyKey
    member Parent: HierarchyKey with get
    member Region: string with get
    member View: string with get
    member Hash: string with get
    override Equals: obj -> bool
    override GetHashCode: unit -> int
    interface IComparable<HierarchyKey>
    interface IComparable
    interface IEquatable<HierarchyKey>

module internal HierarchyKey =
    val add: string -> string -> string -> HierarchyKey -> HierarchyKey

    val addNew: string -> string -> HierarchyKey -> HierarchyKey

    val isShell: HierarchyKey -> bool
