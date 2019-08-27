namespace Forest
{
    public interface ITreeNavigator
    {
        void Navigate(string tree);
        void Navigate<T>(string tree, T message);
    }
}