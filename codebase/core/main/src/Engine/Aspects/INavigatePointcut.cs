using Forest.Navigation;

namespace Forest.Engine.Aspects
{
    /// <summary>
    /// Represents a pointcut in the Forest engine flow which occurs during navigation.
    /// </summary>
    public interface INavigatePointcut : IForestExecutionPointcut
    {
        NavigationInfo NavigationInfo { get; }
    }
}