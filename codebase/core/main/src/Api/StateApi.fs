namespace Forest

/// An interface representing a forest state visitor;
type [<Interface>] IForestStateVisitor =
    abstract member BFS: key:HierarchyKey -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    abstract member DFS: key:HierarchyKey -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    /// Executed once when the traversal is complete.
    abstract member Complete: unit -> unit