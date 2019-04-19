namespace Forest

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
[<NoComparison>] 
type StateChange =
    | ViewAdded of parent : TreeNode * model : obj
    | ViewAddedWithModel of parent : TreeNode * model : obj
    | ModelUpdated of owner : TreeNode * newModel : obj
    | ViewDestroyed of node : TreeNode