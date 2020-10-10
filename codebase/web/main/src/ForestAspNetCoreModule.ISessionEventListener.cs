using Axle.Logging;
using Axle.Web.AspNetCore.Session;
using Microsoft.AspNetCore.Http;

namespace Forest.Web.AspNetCore
{
    partial class ForestAspNetCoreModule : ISessionEventListener
    {
        private readonly ForestSessionStateProvider _sessionStateProvider;
        
        void ISessionEventListener.OnSessionStart(ISession session)
        {
            _sessionStateProvider.CompareReplace(new ForestSessionState(), (a, b) => b);
        }

        void ISessionEventListener.OnSessionEnd(string sessionId)
        {
            if (_sessionStateProvider.TryRemove(out _))
            {
                _logger.Info("Deleted forest session state for session {0}", sessionId);
            }
        }
    }
}
