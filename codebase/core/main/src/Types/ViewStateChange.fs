namespace Forest

open System
open Forest

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
[<NoComparison;Obsolete>] 
type ViewStateChange =
    | ViewAdded of parent : Tree.Node * viewState : ViewState
    | ViewAddedWithModel of parent : Tree.Node * viewState : ViewState
    | ViewStateUpdated of owner : Tree.Node * newState : ViewState
    | ViewDestroyed of node : Tree.Node