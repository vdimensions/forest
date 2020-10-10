namespace Forest.StateManagement
{
    public interface IForestStateProvider
    {
        ForestState LoadState();
        void BeginStateUpdate(ForestState state);
        void EndStateUpdate();
    }
}