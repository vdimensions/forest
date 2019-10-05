namespace Forest.StateManagement
{
    public sealed class DefaultForestStateProvider : IForestStateProvider
    {
        private ForestState _st;

        ForestState IForestStateProvider.LoadState() => _st = _st ?? new ForestState();

        void IForestStateProvider.CommitState(ForestState state) => _st = state;

        void IForestStateProvider.RollbackState() {}
    }
}