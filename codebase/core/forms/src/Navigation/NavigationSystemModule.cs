using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Forms.Navigation.Breadcrumbs;

namespace Forest.Forms.Navigation
{
    [Module]
    internal sealed class NavigationSystemModule : IForestViewProvider
    {
        void IForestViewProvider.RegisterViews(IViewRegistry registry) 
            => registry
                .Register<NavigationMenu.View>()
                    .Register<NavigationMenu.Item.View>()
                    .Register<NavigationMenu.NavigableItem.View>()
                .Register<BreadcrumbsMenu.View>()
                    .Register<BreadcrumbsMenuItem.View>()
                    .Register<BreadcrumbsMenuNavigableItem.View>();
    }
}
