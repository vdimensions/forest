namespace Forest.Engine.Aspects
{
    public interface IForestExecutionPointcut
    {
        /// <summary>
        /// Resumes the Forest engine flow from the current pointcut.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the pointcut execution was successful;
        /// <c>false</c> otherwise.
        /// <para>
        /// When this method returns <c>false</c>, it is an indication for the calling advice that the flow failed to
        /// complete further in the execution pipeline, but the errors were either handled, or suppressed intentionally.
        /// This gives a chance for the calling advice to perform cleanup or rollback operations.
        /// </para>
        /// </returns>
        bool Proceed();
    }
}