namespace Forest.StateManagement
{
    public sealed class DefaultForestStateProvider : IForestStateProvider
    {
        private ForestState _st;

        ForestState IForestStateProvider.BeginUsingState() => _st = _st ?? new ForestState();

        void IForestStateProvider.UpdateState(ForestState state) => _st = state;

        void IForestStateProvider.EndUsingState() { }
    }
}