using Forest.Engine;
using Forest.Engine.Instructions;
using Forest.Navigation;

namespace Forest.Security
{
    public interface IForestSecurityExceptionHandler
    {
        void HandleSecurityException(ForestSecurityException securityException, Location location, IForestEngine forestEngine);
        void HandleSecurityException(ForestSecurityException securityException, string command, IForestEngine forestEngine);
        void HandleSecurityException(ForestSecurityException securityException, IForestEngine forestEngine);
    }
}