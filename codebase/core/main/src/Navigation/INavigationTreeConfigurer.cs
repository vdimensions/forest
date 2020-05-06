using Axle.Modularity;

namespace Forest.Navigation
{
    [Requires(typeof(NavigationModule))]
    public interface INavigationTreeConfigurer
    {
        void Configure(INavigationManager navigationManager);
    }
}