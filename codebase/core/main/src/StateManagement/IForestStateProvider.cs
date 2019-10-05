namespace Forest.StateManagement
{
    public interface IForestStateProvider
    {
        ForestState LoadState();
        void CommitState(ForestState state);
        void RollbackState();
    }
}