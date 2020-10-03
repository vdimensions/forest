namespace Forest.Navigation
{
    public interface ITreeNavigator
    {
        void Navigate(Location location);
        
        void NavigateBack();
        void NavigateBack(int offset);
        
        void NavigateUp();
        void NavigateUp(int offset);
    }
}