using Axle.Modularity;

namespace Forest.Security
{
    [Requires(typeof(ForestSecurityModule))]
    public interface IForestSecurityManagerProvider
    {
        IForestSecurityManager GetSecurityManager();
    }
}