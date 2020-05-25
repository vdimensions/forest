using System.Threading;
using Forest.StateManagement;

namespace Forest.Web.AspNetCore
{
    partial class ForestAspNetCoreModule : IForestStateProvider
    {
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
