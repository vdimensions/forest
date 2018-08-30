namespace Forest

open System

type [<Sealed>] TreeNode =
    [<CompiledName("Shell")>]
    static member shell:TreeNode
    [<CompiledName("NewKey")>]
    static member newKey: region:rname -> view:vname -> parent:TreeNode -> TreeNode
    /// The key for the parent node containing the current entry
    member Parent:TreeNode with get
    /// The region containing the current hierarchy entry
    member Region:rname with get
    /// The name of the view represented by the current entry
    member View:vname with get
    /// A unique identifier for the current entry
    member Hash:hash with get
    member RegionFragment:string with get
    override Equals: o:obj -> bool
    override GetHashCode: unit -> int
    interface IComparable<TreeNode>
    interface IComparable
    interface IEquatable<TreeNode>

module internal TreeNode =
    //val add: string -> string -> string -> HierarchyKey -> HierarchyKey
    //val addNew: string -> string -> HierarchyKey -> HierarchyKey
    val isShell: TreeNode -> bool
