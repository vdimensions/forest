using System.Linq;
using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Navigation;

namespace Forest.Forms.Navigation.Breadcrumbs
{
    public static class BreadcrumbsMenu
    {
        [Module]
        internal sealed class Module : IForestViewProvider
        {
            void IForestViewProvider.RegisterViews(IViewRegistry registry)
            {
                registry
                    .Register<View>()
                    .Register<BreadcrumbsMenuItem.View>()
                    .Register<BreadcrumbsMenuNavigableItem.View>();
            }
        }
        
        private const string Name = "BreadcrumbsMenu";

        private static class Regions
        {
            public const string Items = "Items";
        }

        [View(Name)]
        internal sealed class View : NavigationMenu.AbstractView
        {
            protected override void OnNavigationTreeChanged(NavigationTree tree)
            {
                WithRegion(Regions.Items, itemsRegion =>
                {
                    itemsRegion.Clear();
                    var nodes = tree.SelectedNodes.ToArray();
                    var selectedItems = nodes
                        .Select((x, i) => new NavigationNode { Path = x, Selected = true, Offset = nodes.Length - i - 1 })
                        .ToArray();
                    if (selectedItems.Length == 0)
                    {
                        return;
                    }
                    var last = selectedItems[selectedItems.Length - 1];
                    for (var i = 0; i < selectedItems.Length - 1; i++)
                    {
                        itemsRegion.ActivateView<BreadcrumbsMenuNavigableItem.View, NavigationNode>(selectedItems[i]);
                    }
                    itemsRegion.ActivateView<BreadcrumbsMenuItem.View, NavigationNode>(last);
                });
            }
        }
    }
}
