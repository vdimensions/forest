namespace Forest

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
type [<NoComparison>] StateChange =
    | ViewAdded of parent:TreeNode * model:obj
    | ViewAddedWithModel of parent:TreeNode * model:obj
    | ModelUpdated of owner:TreeNode * newModel:obj
    | ViewDestroyed of node:TreeNode