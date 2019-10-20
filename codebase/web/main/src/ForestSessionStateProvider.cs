using System.Collections.Immutable;
using Axle.Web.AspNetCore.Session;
using Forest.ComponentModel;
using Forest.StateManagement;
using Forest.Web.AspNetCore.Dom;
using Microsoft.AspNetCore.Http;

namespace Forest.Web.AspNetCore
{
    internal sealed class ForestSessionStateProvider : SessionScoped<ForestSessionState>, IClientViewsHelper
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
            var s = Current;
            AddOrReplace(
                sessionId,
                ForestSessionState.ReplaceState(s, forestState),
                (oldState, newState) => newState);
        }
        public void UpdateAllViews(IImmutableDictionary<string, ViewNode> allViews)
        {
            var sessionId = _accessor.HttpContext.Session.Id;
            var s = Current;
            AddOrReplace(
                sessionId,
                ForestSessionState.ReplaceAllViews(s, allViews),
                (oldState, newState) => newState);
        }
        public void UpdateUpdatedViews(IImmutableDictionary<string, ViewNode> updatedViews)
        {
            var sessionId = _accessor.HttpContext.Session.Id;
            var s = Current;
            AddOrReplace(
                sessionId,
                ForestSessionState.ReplaceUpdatedViews(s, updatedViews),
                (oldState, newState) => newState);
        }

        public IViewDescriptor GetDescriptor(string instanceId)
        {
            var state = Current.State;
            return _stateInspector.GetViewDescriptor(state, instanceId);
        }

        public bool TryGetDescriptor(string instanceId, out IViewDescriptor descriptor)
        {
            var state = Current.State;
            return _stateInspector.TryGetViewDescriptor(state, instanceId, out descriptor);
        }

        public IImmutableDictionary<string, ViewNode> AllViews => Current.AllViews;
        public IImmutableDictionary<string, ViewNode> UpdatedViews => Current.UpdatedViews;
    }
}