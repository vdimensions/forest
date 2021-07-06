using Axle.Modularity;

namespace Forest.Security
{
    [Requires(typeof(ForestSecurityModule))]
    public interface IForestSecurityExceptionHandlerProvider
    {
        IForestSecurityExceptionHandler GetSecurityExceptionHandler();
    }
}