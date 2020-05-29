namespace Forest.Engine.Aspects
{
    /// <summary>
    /// Represents a pointcut in the Forest engine flow which occurs during navigation.
    /// </summary>
    public interface INavigatePointcut : IForestExecutionPointcut
    {
        /// <summary>
        /// The target template to navigate to.
        /// </summary>
        string Target { get; }
        /// <summary>
        /// An optional message to act as navigation parameter.
        /// </summary>
        object Message { get; }
    }
}