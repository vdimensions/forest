﻿using System.Collections.Immutable;
using Axle.Web.AspNetCore.Session;
using Forest.ComponentModel;
using Forest.Navigation;
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
       
        public void UpdateNavigationState(NavigationState navigationState)
        {
            var sessionId = _accessor.HttpContext.Session.Id;
            var s = Current;
            AddOrReplace(
                sessionId,
                ForestSessionState.ReplaceNavigationState(s, navigationState),
                (oldState, newState) => newState);
        }

        public IForestViewDescriptor GetDescriptor(string instanceId)
        {
            var state = Current.State;
            return _stateInspector.GetViewDescriptor(state, instanceId);
        }

        public bool TryGetDescriptor(string instanceId, out IForestViewDescriptor descriptor)
        {
            var state = Current.State;
            return _stateInspector.TryGetViewDescriptor(state, instanceId, out descriptor);
        }

        public NavigationState NavigationState => Current.NavigationState;
        public IImmutableDictionary<string, ViewNode> AllViews => Current.AllViews;
        public IImmutableDictionary<string, ViewNode> UpdatedViews => Current.UpdatedViews;
    }
}