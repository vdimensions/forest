using System;
using Axle.Web.AspNetCore.Session;
using Forest.Collections.Immutable;
using Forest.ComponentModel;
using Forest.Navigation;
using Forest.StateManagement;
using Forest.Web.AspNetCore.Dom;

namespace Forest.Web.AspNetCore
{
    internal sealed class ForestSessionStateProvider : IClientViewsHelper
    {
        private readonly SessionReference<ForestSessionState> _stateReference;
        private readonly IForestStateInspector _stateInspector;

        public ForestSessionStateProvider(ISessionReferenceProvider sessionReferenceProvider, IForestStateInspector stateInspector)
        {
            _stateReference = sessionReferenceProvider.CreateSessionReference<ForestSessionState>();
            _stateInspector = stateInspector;
        }

        public void UpdateState(ForestState forestState)
        {
            var s = _stateReference.Value;
            _stateReference.CompareReplace(ForestSessionState.ReplaceState(s, forestState), (oldState, newState) => newState);
        }

        public void CompareReplace(ForestSessionState forestSessionState, Func<ForestSessionState, ForestSessionState, ForestSessionState> func) 
            => _stateReference.CompareReplace(forestSessionState, func);
       
        public bool TryGetViewDescriptor(string instanceId, out IForestViewDescriptor descriptor)
        {
            if (_stateReference.TryGetValue(out var value))
            {
                var state = value;
                return _stateInspector.TryGetViewDescriptor(state.State, instanceId, out descriptor);
            }
            descriptor = null;
            return false;
        }

        public Location Location => _stateReference.Value.State.Location;
        public ImmutableDictionary<string, ViewNode> AllViews => _stateReference.Value.AllViews;
        public ImmutableDictionary<string, ViewNode> UpdatedViews => _stateReference.Value.UpdatedViews;
        public ForestSessionState Value => _stateReference.Value;
    }
}