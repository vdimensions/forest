namespace Forest

open System

type [<Sealed>] HierarchyKey =
    [<CompiledName("Shell")>]
    static member shell:HierarchyKey
    [<CompiledName("NewKey")>]
    static member newKey: region:rname -> view:vname -> parent:HierarchyKey -> HierarchyKey
    /// The key for the parent node containing the current entry
    member Parent:HierarchyKey with get
    /// The region containing the current hierarchy entry
    member Region:rname with get
    /// The name of the view represented by the current entry
    member View:vname with get
    /// A unique identifier for the current entry
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
