﻿namespace Forest

/// An interface representing a forest state visitor
type [<Interface>] internal IForestStateVisitor =
    /// Called upon visiting a sibling or child BFS-style
    abstract member BFS: key:TreeNode -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    /// Called upon visiting a sibling or child DFS-style
    abstract member DFS: key:TreeNode -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    /// Executed once when the traversal is complete.
    abstract member Complete: unit -> unit