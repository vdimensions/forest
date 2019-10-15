using Axle.Logging;
using Axle.Modularity;
using Axle.Web.AspNetCore;
using Axle.Web.AspNetCore.Mvc;
using Axle.Web.AspNetCore.Session;
using Forest.Engine;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Forest.Web.AspNetCore
{
    [Module]
    [RequiresForest]
    [RequiresAspNetSession]
    [RequiresAspNetMvc]
    internal sealed class ForestAspNetCoreModule : ISessionEventListener, IServiceConfigurer
    {
        private readonly IForestEngine _forestEngine;
        private readonly ForestSessionStateProvider _sessionStateProvider;
        private readonly ILogger _logger;

        public ForestAspNetCoreModule(IForestEngine forestEngine, IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            _forestEngine = forestEngine;
            _sessionStateProvider = new ForestSessionStateProvider(httpContextAccessor);
            _logger = logger;
        }

        void IServiceConfigurer.Configure(IServiceCollection services)
        {
            services.AddSingleton(_forestEngine);
        }

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
    }
}
