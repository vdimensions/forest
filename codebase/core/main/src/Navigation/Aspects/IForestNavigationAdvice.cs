using Forest.Engine.Aspects;

namespace Forest.Navigation.Aspects
{
    public interface IForestNavigationAdvice
    {
        bool Navigate(INavigatePointcut pointcut);
    }
}