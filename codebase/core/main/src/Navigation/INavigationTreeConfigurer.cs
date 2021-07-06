using Axle.Modularity;

namespace Forest.Navigation
{
    [Requires(typeof(ForestNavigationModule))]
    public interface INavigationTreeConfigurer
    {
        void Configure(INavigationManager navigationManager);
    }
}