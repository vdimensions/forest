using System;
using System.Collections.Immutable;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using Forest.ComponentModel;
using Forest.Engine;
using Forest.UI;

namespace Forest.StateManagement
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class ForestState
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly Guid _stateID;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly Tree _tree;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly ImmutableDictionary<string, ViewState> _viewStates;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [IgnoreDataMember]
        #endif
        private readonly ImmutableDictionary<string, IRuntimeView> _logicalViews;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [IgnoreDataMember]
        #endif
        private readonly ImmutableDictionary<string, IPhysicalView> _physicalViews;

        internal ForestState(
            Guid stateID,
            Tree tree,
            ImmutableDictionary<string, ViewState> viewStates, 
            ImmutableDictionary<string, IRuntimeView> logicalViews,
            ImmutableDictionary<string, IPhysicalView> physicalViews)
        {
            _stateID = stateID;
            _tree = tree;
            _viewStates = viewStates;

            _logicalViews = logicalViews;
            _physicalViews = physicalViews;
        }
        public ForestState() : this(Guid.Empty, Tree.Root, ImmutableDictionary<string, ViewState>.Empty, ImmutableDictionary<string, IRuntimeView>.Empty, ImmutableDictionary<string, IPhysicalView>.Empty) { }

        internal Guid StateID => _stateID;
        internal Tree Tree => _tree;
        internal ImmutableDictionary<string, ViewState> ViewStates => _viewStates;
        internal ImmutableDictionary<string, IRuntimeView> LogicalViews => _logicalViews;
        internal ImmutableDictionary<string, IPhysicalView> PhysicalViews => _physicalViews;
    }

    public interface IForestStateProvider
    {
        ForestState LoadState();
        void CommitState(ForestState state);
        void RollbackState();
    }

    /// An interface representing a forest state visitor
    internal interface IForestStateVisitor
    {
        /// Called upon visiting a sibling or child BFS-style
        void BFS(Tree.Node node, int index, ViewState viewState, IViewDescriptor descriptor);
        /// Called upon visiting a sibling or child DFS-style
        void DFS(Tree.Node node, int index, ViewState viewState, IViewDescriptor descriptor);
        /// Executed once when the traversal is complete.
        void Complete();
    }
    
}