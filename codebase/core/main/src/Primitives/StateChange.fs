namespace Forest

open System

[<Serializable>]
type [<Struct>] StateChange =
    | ViewAdded of parent:TreeNode * model:obj
    | ViewModelUpdated of owner:TreeNode * newModel:obj
    | ViewDestroyed of node:TreeNode