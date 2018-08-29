namespace Forest

open System

[<Serializable>]
type [<Struct>] StateChange =
    | ViewAdded of parent:HierarchyKey * viewModel:obj
    | ViewModelUpdated of id:HierarchyKey * updatedViewModel:obj
    | ViewDestroyed of destroyedViewID:HierarchyKey