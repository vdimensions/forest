using Forest.Engine;
using Forest.Engine.Instructions;
using Forest.Navigation;

namespace Forest.Security
{
    internal sealed class DefaultForestSecurityExceptionHandler : IForestSecurityExceptionHandler
    {
        void IForestSecurityExceptionHandler.HandleSecurityException(ForestSecurityException securityException, NavigationTarget navigationTarget, IForestEngine forestEngine)
            => HandleSecurityException(securityException, forestEngine);

        void IForestSecurityExceptionHandler.HandleSecurityException(ForestSecurityException securityException, string command, IForestEngine forestEngine)
            => HandleSecurityException(securityException, forestEngine);

        public void HandleSecurityException(ForestSecurityException securityException, IForestEngine forestEngine)
            => throw securityException;
    }
}