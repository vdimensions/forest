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
            return new ForestSessionState(forestState, sessionState.SyncRoot, sessionState.NavigationState, sessionState.AllViews, sessionState.UpdatedViews);
        }
        public static ForestSessionState ReplaceNavigationState(ForestSessionState sessionState, NavigationState navigationState)
        {
            return new ForestSessionState(sessionState.State, sessionState.SyncRoot, navigationState, sessionState.AllViews, sessionState.UpdatedViews);
        }

        private ForestSessionState(
            ForestState state, 
            object syncRoot, 
            NavigationState navigationState,
            IImmutableDictionary<string, ViewNode> allViews, 
            IImmutableDictionary<string, ViewNode> updatedViews)
        {
            allViews = ImmutableDictionary.CreateRange(
                state.PhysicalViews
                    .Select(x => new KeyValuePair<string, ViewNode>(x.Key, ((WebApiPhysicalView) x.Value).Node)));
            updatedViews = allViews;
            State = state;
            SyncRoot = syncRoot;
            NavigationState = navigationState;
            AllViews = allViews;
            UpdatedViews = updatedViews;
        }
        private ForestSessionState(IEqualityComparer<string> stringComparer) 
            : this(
                new ForestState(), 
                new object(), 
                NavigationState.Empty, 
                ImmutableDictionary.Create<string, ViewNode>(stringComparer), 
                ImmutableDictionary.Create<string, ViewNode>(stringComparer)) { }
        internal ForestSessionState() : this(StringComparer.Ordinal) { }

        public ForestState State { get; }
        internal object SyncRoot { get; }
        internal NavigationState NavigationState { get; }
        internal IImmutableDictionary<string, ViewNode> AllViews { get; }
        internal IImmutableDictionary<string, ViewNode> UpdatedViews { get; }
    }
}