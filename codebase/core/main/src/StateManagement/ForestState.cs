﻿using System;
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
        private readonly NavigationInfo _navigationInfo;
        
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
            NavigationInfo navigationInfo,
            Tree tree,
            ImmutableDictionary<string, ViewState> viewStates, 
            ImmutableDictionary<string, IRuntimeView> logicalViews,
            ImmutableDictionary<string, IPhysicalView> physicalViews)
        {
            _stateID = stateID;
            _navigationInfo = navigationInfo;
            _tree = tree;
            _viewStates = viewStates;

            _logicalViews = logicalViews;
            _physicalViews = physicalViews;
        }
        public ForestState() : this(Guid.Empty, new NavigationInfo(), Tree.Root, ImmutableDictionary<string, ViewState>.Empty, ImmutableDictionary<string, IRuntimeView>.Empty, ImmutableDictionary<string, IPhysicalView>.Empty) { }

        internal Guid StateID => _stateID;
        public NavigationInfo NavigationInfo => _navigationInfo;
        internal Tree Tree => _tree;
        internal ImmutableDictionary<string, ViewState> ViewStates => _viewStates;
        internal ImmutableDictionary<string, IRuntimeView> LogicalViews => _logicalViews;
        internal ImmutableDictionary<string, IPhysicalView> PhysicalViews => _physicalViews;
    }
}