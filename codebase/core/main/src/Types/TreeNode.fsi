namespace Forest

open System

/// A class representing the view composition hierarchy supported by Forest.
/// The hierarchy consists of views, each view having a number of regions, and each region being able to host numerous child views.
/// The structure is non-cyclic.
/// For simplicity, the region hosting a particular view is represented as a member of the tree node for that view.
type [<Sealed>] TreeNode =
    /// Represents the root entry in the view composition hierarchy; a.k.a. the 'Shell' view
    [<CompiledName("Shell")>]
    static member shell:TreeNode
    [<CompiledName("NewKey")>]
    /// Generates a new key and a corresponding entry in the view composition hierarchy
    static member newKey: region:rname -> view:vname -> parent:TreeNode -> TreeNode
    /// A reference to the current node's parent in the view composition hierarchy
    member Parent:TreeNode with get
    /// The name of the region that hosts the view object represented by the current node
    member Region:rname with get
    /// The name of the view represented by the current node
    member View:vname with get
    /// A unique identifier for the current hierarchy entry. Can be used for quick access to this hierarchy element without the need of traversal,
    /// as well as a key in a Map or Set
    member Hash:thash with get
    /// A string that represents the hierarchy path from the Shell (root) of the view composition to the region (including) that houses the current view.
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
