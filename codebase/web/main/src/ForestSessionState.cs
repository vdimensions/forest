using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Forest.Navigation;
using Forest.StateManagement;
using Forest.Web.AspNetCore.Dom;

namespace Forest.Web.AspNetCore
{
    [Serializable]
    internal sealed class ForestSessionState
    {
        public static ForestSessionState ReplaceState(ForestSessionState sessionState, ForestState forestState)
        {
            return new ForestSessionState(forestState, sessionState.SyncRoot, sessionState.NavigationInfo, sessionState.AllViews, sessionState.UpdatedViews);
        }
        public static ForestSessionState ReplaceNavigationInfo(ForestSessionState sessionState, NavigationInfo navigationInfo)
        {
            return new ForestSessionState(sessionState.State, sessionState.SyncRoot, navigationInfo, sessionState.AllViews, sessionState.UpdatedViews);
        }

        private ForestSessionState(
            ForestState state, 
            object syncRoot, 
            NavigationInfo navigationInfo,
            IImmutableDictionary<string, ViewNode> allViews, 
            IImmutableDictionary<string, ViewNode> updatedViews)
        {
            allViews = ImmutableDictionary.CreateRange(
                state.PhysicalViews
                    .Select(x => new KeyValuePair<string, ViewNode>(x.Key, ((WebApiPhysicalView) x.Value).Node)));
            updatedViews = allViews;
            State = state;
            SyncRoot = syncRoot;
            NavigationInfo = navigationInfo;
            AllViews = allViews;
            UpdatedViews = updatedViews;
        }
        internal ForestSessionState() 
            : this(
                new ForestState(), 
                new object(), 
                NavigationInfo.Empty, 
                ImmutableDictionary.Create<string, ViewNode>(StringComparer.Ordinal), 
                ImmutableDictionary.Create<string, ViewNode>(StringComparer.Ordinal)) { }

        public ForestState State { get; }
        internal object SyncRoot { get; }
        internal NavigationInfo NavigationInfo { get; }
        internal IImmutableDictionary<string, ViewNode> AllViews { get; }
        internal IImmutableDictionary<string, ViewNode> UpdatedViews { get; }
    }
}