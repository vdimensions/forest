using Axle.Logging;
using Axle.Modularity;
using Axle.Web.AspNetCore.Session;
using Forest.Engine;
using Microsoft.AspNetCore.Http;

namespace Forest.Web.AspNetCore
{
    [Module]
    [RequiresForest]
    [RequiresAspNetSession]
    internal sealed class ForestAspNetCoreModule : ISessionEventListener
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
