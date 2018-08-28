namespace Forest

open System

type rname = string
type vname = string
type cname = string
type sname = string

type [<Sealed>] HierarchyKey =
    [<CompiledName("Shell")>]
    static member shell:HierarchyKey
    [<CompiledName("NewKey")>]
    static member newKey: region:rname -> view:vname -> parent:HierarchyKey -> HierarchyKey
    member Parent:HierarchyKey with get
    member Region:rname with get
    member View:vname with get
    member Hash:sname with get
    member RegionFragment:string with get
    override Equals: o:obj -> bool
    override GetHashCode: unit -> int
    interface IComparable<HierarchyKey>
    interface IComparable
    interface IEquatable<HierarchyKey>

module internal HierarchyKey =
    //val add: string -> string -> string -> HierarchyKey -> HierarchyKey
    //val addNew: string -> string -> HierarchyKey -> HierarchyKey
    val isShell: HierarchyKey -> bool
