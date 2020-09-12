using Forest.Engine.Aspects;

namespace Forest.Web.AspNetCore
{
    partial class ForestAspNetCoreModule : IForestNavigationAdvice
    {
        public bool Navigate(INavigatePointcut pointcut)
        {
            if (pointcut.Proceed())
            {
                _sessionStateProvider.UpdateNavigationState(pointcut.NavigationState);
                return true;
            }
            return false;
        }
    }
}
