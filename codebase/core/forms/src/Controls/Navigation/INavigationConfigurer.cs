using Axle.Modularity;

namespace Forest.Forms.Controls.Navigation
{
    [Requires(typeof(ForestNavigationSystemModule))]
    public interface INavigationConfigurer
    {
        INavigationConfigurer Configure(INavigationBuilder builder);
    }
}