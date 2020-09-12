namespace Forest.Engine.Aspects
{
    public interface IForestExecutionPointcut
    {
        /// <summary>
        /// Resumes the Forest engine flow from the current pointcut.
        /// </summary>
        bool Proceed();
    }
}