using System;
using System.Threading;
using Axle.Logging;
using Axle.Modularity;
using Axle.Web.AspNetCore;
using Axle.Web.AspNetCore.Mvc.ModelBinding;
using Axle.Web.AspNetCore.Session;
using Forest.ComponentModel;
using Forest.Engine;
using Forest.StateManagement;
using Forest.UI;
using Forest.Web.AspNetCore.Controllers;
using Forest.Web.AspNetCore.Dom;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ILogger = Axle.Logging.ILogger;

namespace Forest.Web.AspNetCore
{
    [Module]
    [RequiresForest]
    internal sealed class ForestAspNetCoreModule : ForestEngineContextProvider, ISessionEventListener, IServiceConfigurer, IModelResolverProvider, IForestStateProvider
    {
        private readonly IForestEngine _forestEngine;
        private readonly IViewRegistry _viewRegistry;
        private readonly ForestSessionStateProvider _sessionStateProvider;
        private readonly ILogger _logger;

        public ForestAspNetCoreModule(IForestEngine forestEngine, IViewRegistry viewRegistry, IHttpContextAccessor httpContextAccessor, IForestStateInspector stateInspector, ILogger logger)
        {
            _forestEngine = forestEngine;
            _viewRegistry = viewRegistry;
            _sessionStateProvider = new ForestSessionStateProvider(httpContextAccessor, stateInspector);
            _logger = logger;
        }

        void IServiceConfigurer.Configure(IServiceCollection services) => services.AddSingleton(_forestEngine).AddSingleton<IClientViewsHelper>(_sessionStateProvider);

        void ISessionEventListener.OnSessionStart(ISession session)
        {
            var sessionId = session.Id;
            _sessionStateProvider.AddOrReplace(sessionId, new ForestSessionState(), (a, b) => b);
        }

        void ISessionEventListener.OnSessionEnd(string sessionId)
        {
            if (_sessionStateProvider.TryRemove(sessionId, out _))
            {
                _logger.Trace("Deleted forest session state for session {0}", sessionId);
            }
        }

        public void RegisterTypes(IModelTypeRegistry registry)
        {
            foreach (var descriptor in _viewRegistry.Descriptors)
            {
                registry.Register(descriptor.ModelType);
                foreach (var eventDescriptor in descriptor.Events)
                {
                    if (!string.IsNullOrEmpty(eventDescriptor.Topic))
                    {
                        continue;
                    }
                    registry.Register(eventDescriptor.MessageType);
                }
                foreach (var commandDescriptor in descriptor.Commands.Values)
                {
                    if (commandDescriptor.ArgumentType == null)
                    {
                        continue;
                    }
                    registry.Register(commandDescriptor.ArgumentType);
                }
            }
        }

        IModelResolver IModelResolverProvider.GetModelResolver(Type modelType)
        {
            if (modelType == typeof(IForestMessageArg))
            {
                return new ForestMessageResolver(_sessionStateProvider);
            }
            if (modelType == typeof(IForestCommandArg))
            {
                return new ForestCommandResolver(_sessionStateProvider);
            }
            return null;
        }

        protected override IPhysicalViewRenderer GetPhysicalViewRenderer() => new WebApiPhysicalViewRenderer(_sessionStateProvider);
        protected override IForestStateProvider GetForestStateProvider() => this;

        ForestState IForestStateProvider.LoadState()
        {
            var state = _sessionStateProvider.Current;
            Monitor.Enter(state.SyncRoot);
            return state.State;
        }

        void IForestStateProvider.CommitState(ForestState state)
        {
            var s = _sessionStateProvider.Current;
            try
            {
                _sessionStateProvider.UpdateState(state);
            }
            finally
            {
                Monitor.Exit(s.SyncRoot);
            }
        }

        void IForestStateProvider.RollbackState()
        {
            var s = _sessionStateProvider.Current;
            Monitor.Exit(s.SyncRoot);
        }
    }
}
