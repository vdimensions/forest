using Forest.Engine.Aspects;

namespace Forest.Web.AspNetCore
{
    partial class ForestAspNetCoreModule : IForestNavigationAdvice
    {
        public void Navigate(INavigatePointcut pointcut)
        {
            pointcut.Proceed();
            _sessionStateProvider.UpdateNavigationState(pointcut.NavigationState);
        }
    }
}
