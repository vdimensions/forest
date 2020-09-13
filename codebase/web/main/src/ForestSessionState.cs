using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Axle.Collections.Generic.Extensions.KeyValuePair;
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
            return new ForestSessionState(forestState, sessionState.SyncRoot, sessionState.NavigationState, StringComparer.Ordinal);
        }
        public static ForestSessionState ReplaceNavigationState(ForestSessionState sessionState, NavigationState navigationState)
        {
            return new ForestSessionState(sessionState.State, sessionState.SyncRoot, navigationState, sessionState.AllViews, sessionState.UpdatedViews);
        }

        private static ViewNode CreateEmptyViewNode(ViewNode node)
        {
            return new ViewNode
            {
                ID = node.ID,
                Regions = new Dictionary<string, string[]>(),
                Commands = new Dictionary<string, CommandNode>()
            };
        }

        private ForestSessionState(
            ForestState state, 
            object syncRoot, 
            NavigationState navigationState,
            IImmutableDictionary<string, ViewNode> allViews, 
            IImmutableDictionary<string, ViewNode> updatedViews)
        {
            State = state;
            SyncRoot = syncRoot;
            NavigationState = navigationState;

            if (allViews.Count == 0)
            {
                var comparer = state.PhysicalViews.KeyComparer;
                var allPairs = new List<KeyValuePair<string, WebApiPhysicalView>>();
                var allViewsRange = new List<KeyValuePair<string, ViewNode>>();
                var updatedViewsRange = new List<KeyValuePair<string, ViewNode>>();
                var maxRevision = 0u;
                foreach (var pair in state.PhysicalViews)
                {
                    var viewPair = pair.MapValue(v => (WebApiPhysicalView) v);
                    var nodePair = viewPair.MapValue(v => v.Node);
                    maxRevision = Math.Max(maxRevision, viewPair.Value.Revision);
                    allViewsRange.Add(nodePair);
                    allPairs.Add(viewPair);
                }
                allViews = ImmutableDictionary.CreateRange(comparer, allViewsRange);
                foreach (var pair in allPairs)
                {
                    updatedViewsRange.Add(pair.MapValue(x => x.Revision == maxRevision ? x.Node : CreateEmptyViewNode(x.Node)));
                }
                updatedViews = ImmutableDictionary.CreateRange(comparer, updatedViewsRange);
            }
            AllViews = allViews;
            UpdatedViews = updatedViews;
        }
        private ForestSessionState(
                ForestState state, 
                object syncRoot, 
                NavigationState navigationState,
                IEqualityComparer<string> stringComparer) 
            : this(
                state, 
                syncRoot, 
                navigationState, 
                ImmutableDictionary.Create<string, ViewNode>(stringComparer), 
                ImmutableDictionary.Create<string, ViewNode>(stringComparer)) { }
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