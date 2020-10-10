using System.Threading;
using Forest.StateManagement;

namespace Forest.Web.AspNetCore
{
    partial class ForestAspNetCoreModule : IForestStateProvider
    {
        ForestState IForestStateProvider.BeginUsingState()
        {
            var state = _sessionStateProvider.Current;
            Monitor.Enter(state.SyncRoot);
            return state.State;
        }

        void IForestStateProvider.UpdateState(ForestState state)
        {
            var s = _sessionStateProvider.Current;
            _sessionStateProvider.UpdateState(state);
        }

        void IForestStateProvider.EndUsingState()
        {
            var s = _sessionStateProvider.Current;
            Monitor.Exit(s.SyncRoot);
        }
    }
}
