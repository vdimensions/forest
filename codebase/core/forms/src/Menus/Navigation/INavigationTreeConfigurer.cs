using Axle.Modularity;

namespace Forest.Forms.Menus.Navigation
{
    [Requires(typeof(NavigationSystemModule))]
    public interface INavigationTreeConfigurer
    {
        void Configure(INavigationTreeManager navigationTreeManager);
    }
}