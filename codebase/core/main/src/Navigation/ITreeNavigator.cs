namespace Forest.Navigation
{
    public interface ITreeNavigator
    {
        void Navigate(string path);
        void Navigate<T>(string path, T state);
        
        void NavigateBack();
        void NavigateBack(int offset);
        
        void NavigateUp();
        void NavigateUp(int offset);
    }
}