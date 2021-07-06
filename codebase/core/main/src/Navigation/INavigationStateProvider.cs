namespace Forest.Navigation
{
    public interface INavigationStateProvider
    {
        object ApplyNavigationState(string path);
    }
}