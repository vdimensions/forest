namespace Forest

open System

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
[<NoComparison>] 
type ViewStateChange =
    | ViewAdded of parent : TreeNode * viewState : ViewState
    | ViewAddedWithModel of parent : TreeNode * viewState : ViewState
    | ViewStateUpdated of owner : TreeNode * newState : ViewState
    | ViewDestroyed of node : TreeNode