using Forest.Engine.Aspects;

namespace Forest.Navigation.Aspects
{
    /// <summary>
    /// Represents a pointcut in the Forest engine flow which occurs during navigation.
    /// </summary>
    public interface INavigatePointcut : IForestExecutionPointcut
    {
        /// <summary>
        /// A <see cref="Location"/> instance representing the navigation operation that was intercepted.
        /// </summary>
        Location Location { get; }
    }
}