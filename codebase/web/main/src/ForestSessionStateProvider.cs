using System.Collections.Immutable;
using Axle.Web.AspNetCore.Session;
using Forest.ComponentModel;
using Forest.Navigation;
using Forest.StateManagement;
using Forest.Web.AspNetCore.Dom;
using Microsoft.AspNetCore.Http;

namespace Forest.Web.AspNetCore
{
    internal sealed class ForestSessionStateProvider : SessionReference<ForestSessionState>, IClientViewsHelper
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IForestStateInspector _stateInspector;

        public ForestSessionStateProvider(IHttpContextAccessor accessor, IForestStateInspector stateInspector) : base(accessor)
        {
            _accessor = accessor;
            _stateInspector = stateInspector;
        }

        public void UpdateState(ForestState forestState)
        {
            var sessionId = _accessor.HttpContext.Session.Id;
            var s = Value;
            CompareReplace(ForestSessionState.ReplaceState(s, forestState), (oldState, newState) => newState);
        }
       
        public bool TryGetViewDescriptor(string instanceId, out IForestViewDescriptor descriptor)
        {
            var state = Value.State;
            return _stateInspector.TryGetViewDescriptor(state, instanceId, out descriptor);
        }

        public Location Location => Value.State.Location;
        public IImmutableDictionary<string, ViewNode> AllViews => Value.AllViews;
        public IImmutableDictionary<string, ViewNode> UpdatedViews => Value.UpdatedViews;
    }
}