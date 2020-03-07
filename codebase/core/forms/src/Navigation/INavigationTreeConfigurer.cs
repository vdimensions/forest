using Axle.Modularity;

namespace Forest.Forms.Navigation
{
    [Requires(typeof(NavigationSystemModule))]
    public interface INavigationTreeConfigurer
    {
        void Configure(INavigationTreeManager navigationTreeManager);
    }
}