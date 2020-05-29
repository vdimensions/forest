namespace Forest.Engine.Aspects
{
    public interface IForestNavigationAdvice
    {
        void Navigate(INavigatePointcut pointcut);
    }
}