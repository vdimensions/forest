using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest.Forms.Controls.Navigation
{
    [Module]
    public sealed class NavigationSystemModule : IForestViewProvider
    {
        public void RegisterViews(IViewRegistry registry)
        {
            registry
                .Register<NavigationSystem.View>()
                .Register<Navigation.View>().Register<Navigation.Item.View>()
                .Register<Breadcrumbs.View>().Register<Breadcrumbs.Item.View>()
                ;
        }
    }
}
