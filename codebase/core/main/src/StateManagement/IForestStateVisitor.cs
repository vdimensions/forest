using Forest.ComponentModel;

namespace Forest.StateManagement
{
    /// An interface representing a forest state visitor
    internal interface IForestStateVisitor
    {
        /// Called upon visiting a sibling or child BFS-style
        void BFS(Tree.Node node, int index, IForestViewDescriptor descriptor);
        /// Called upon visiting a sibling or child DFS-style
        void DFS(Tree.Node node, int index, IForestViewDescriptor descriptor);
        /// Executed once when the traversal is complete.
        void Complete();
    }
}