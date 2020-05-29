using Forest.Engine.Aspects;

namespace Forest.Web.AspNetCore
{
    partial class ForestAspNetCoreModule : IForestNavigationAdvice
    {
        public void Navigate(INavigatePointcut pointcut)
        {
            _sessionStateProvider.UpdateNavigationInfo(pointcut.NavigationInfo);
            pointcut.Proceed();
        }
    }
}
