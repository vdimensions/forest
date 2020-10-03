using Axle.Verification;

namespace Forest.Navigation
{
    public static class TreeNavigatorExtensions
    {
        public static void Navigate(this ITreeNavigator navigator, string path)
        {
            navigator.VerifyArgument(nameof(navigator)).IsNotNull();
            path.VerifyArgument(nameof(path)).IsNotNullOrEmpty();
            navigator.Navigate(Location.FromPath(path));
        }

        public static void Navigate<T>(this ITreeNavigator navigator, string path, T state)
        {
            navigator.VerifyArgument(nameof(navigator)).IsNotNull();
            path.VerifyArgument(nameof(path)).IsNotNullOrEmpty();
            state.VerifyArgument(nameof(state)).IsNotNull();
            navigator.Navigate(Location.Create(path, state));
        }
    }
}