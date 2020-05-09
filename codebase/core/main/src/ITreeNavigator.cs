namespace Forest
{
    public interface ITreeNavigator
    {
        void Navigate(string template);
        void Navigate<T>(string template, T message);
        void NavigateBack();
        void NavigateUp();
    }
}