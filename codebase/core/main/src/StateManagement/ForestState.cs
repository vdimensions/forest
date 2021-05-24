using System;
using System.Collections.Generic;
using Axle.Collections.Immutable;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using Forest.Navigation;
using Forest.UI;
using ViewContextTuple = System.Tuple<Forest.Engine._ForestViewContext, Forest.Engine.IRuntimeView>;

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
        private readonly ImmutableDictionary<string, ViewContextTuple> _logicalViews;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [IgnoreDataMember]
        #endif
        private readonly IReadOnlyDictionary<string, IPhysicalView> _physicalViews;

        private readonly Location _location;

        internal ForestState(
            Guid stateID, 
            Location location,
            Tree tree,
            ImmutableDictionary<string, ViewContextTuple> logicalViews,
            IReadOnlyDictionary<string, IPhysicalView> physicalViews)
        {
            _stateID = stateID;
            _location = location;
            _tree = tree;
            _logicalViews = logicalViews;
            _physicalViews = physicalViews;
        }
        public ForestState() : this(Guid.Empty, Location.Empty, Tree.Root, ImmutableDictionary<string, ViewContextTuple>.Empty, ImmutableDictionary<string, IPhysicalView>.Empty) { }

        internal Guid StateID => _stateID;
        internal Tree Tree => _tree;
        internal ImmutableDictionary<string, ViewContextTuple> LogicalViews => _logicalViews;
        public IReadOnlyDictionary<string, IPhysicalView> PhysicalViews => _physicalViews;
        public Location Location => _location;
    }
}