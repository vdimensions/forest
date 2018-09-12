namespace Forest

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
type [<Struct>] StateChange =
    | ViewAdded of parent:TreeNode * model:obj
    | ViewModelUpdated of owner:TreeNode * newModel:obj
    | ViewDestroyed of node:TreeNode