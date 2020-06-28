using System;
using System.Collections.Immutable;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif
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
            ImmutableDictionary<string, IRuntimeView> logicalViews,
            ImmutableDictionary<string, IPhysicalView> physicalViews)
        {
            _stateID = stateID;
            _tree = tree;
            _logicalViews = logicalViews;
            _physicalViews = physicalViews;
        }
        public ForestState() : this(Guid.Empty, Tree.Root, ImmutableDictionary<string, IRuntimeView>.Empty, ImmutableDictionary<string, IPhysicalView>.Empty) { }

        internal Guid StateID => _stateID;
        internal Tree Tree => _tree;
        internal ImmutableDictionary<string, IRuntimeView> LogicalViews => _logicalViews;
        public ImmutableDictionary<string, IPhysicalView> PhysicalViews => _physicalViews;
    }
}