namespace Forest.StateManagement
{
    public interface IForestStateProvider
    {
        ForestState BeginUsingState();
        void UpdateState(ForestState state);
        void EndUsingState();
    }
}