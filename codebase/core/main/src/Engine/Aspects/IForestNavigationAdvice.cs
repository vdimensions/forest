namespace Forest.Engine.Aspects
{
    public interface IForestNavigationAdvice
    {
        bool Navigate(INavigatePointcut pointcut);
    }
}