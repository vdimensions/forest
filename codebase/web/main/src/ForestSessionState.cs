using System;
using System.Collections.Immutable;
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
            return new ForestSessionState(forestState, sessionState.SyncRoot, sessionState.AllViews, sessionState.UpdatedViews);
        }
        public static ForestSessionState ReplaceAllViews(ForestSessionState sessionState, IImmutableDictionary<string, ViewNode> views)
        {
            return new ForestSessionState(sessionState.State, sessionState.SyncRoot, views, views);
        }
        public static ForestSessionState ReplaceUpdatedViews(ForestSessionState sessionState, IImmutableDictionary<string, ViewNode> views)
        {
            return new ForestSessionState(sessionState.State, sessionState.SyncRoot, sessionState.AllViews, views);
        }

        private ForestSessionState(ForestState state, object syncRoot, IImmutableDictionary<string, ViewNode> allViews, IImmutableDictionary<string, ViewNode> updatedViews)
        {
            State = state;
            SyncRoot = syncRoot;
            AllViews = allViews;
            UpdatedViews = updatedViews;
        }
        internal ForestSessionState() : this(
            new ForestState(), 
            new object(), 
            ImmutableDictionary.Create<string, ViewNode>(StringComparer.Ordinal),
            ImmutableDictionary.Create<string, ViewNode>(StringComparer.Ordinal)) { }

        public ForestState State { get; }
        internal object SyncRoot { get; }
        internal NavigationInfo NavigationInfo => State.NavigationInfo;
        internal IImmutableDictionary<string, ViewNode> AllViews { get; }
        internal IImmutableDictionary<string, ViewNode> UpdatedViews { get; }
    }
}