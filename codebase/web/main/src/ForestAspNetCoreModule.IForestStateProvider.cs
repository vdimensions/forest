using System.Threading;
using Forest.StateManagement;

namespace Forest.Web.AspNetCore
{
    partial class ForestAspNetCoreModule : IForestStateProvider
    {
        ForestState IForestStateProvider.BeginUsingState()
        {
            var state = _sessionStateProvider.Value;
            Monitor.Enter(state.SyncRoot);
            return state.State;
        }

        void IForestStateProvider.UpdateState(ForestState state)
        {
            _sessionStateProvider.UpdateState(state);
        }

        void IForestStateProvider.EndUsingState()
        {
            var s = _sessionStateProvider.Value;
            Monitor.Exit(s.SyncRoot);
        }
    }
}
