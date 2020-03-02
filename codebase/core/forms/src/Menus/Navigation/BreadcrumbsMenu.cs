using System.Linq;

namespace Forest.Forms.Menus.Navigation
{
    public static partial class BreadcrumbsMenu
    {
        private const string Name = "ForestBreadcrumbsMenu";

        private static class Regions
        {
            public const string Items = "Items";
        }

        [View(Name)]
        internal sealed class View : NavigationMenu.AbstractView
        {
            internal View(
                    INotifyNavigationTreeChanged notifyNavigationTreeChanged, 
                    INavigationTreeBuilder navigationTreeBuilder) 
                : base(notifyNavigationTreeChanged, navigationTreeBuilder) { }

            protected override void OnNavigationTreeChanged(NavigationTree tree)
            {
                var selectedItems = tree.SelectedNodes.ToArray();
                if (selectedItems.Length == 0)
                {
                    return;
                }

                var itemsRegion = FindRegion(Regions.Items).Clear();
                var last = selectedItems[selectedItems.Length - 1];
                for (var index = 0; index < selectedItems.Length - 1; index++)
                {
                    var item = selectedItems[index];
                    itemsRegion.ActivateView<BreadcrumbsMenu.NavigableItem.View, MenuItemModel>(new MenuItemModel {ID = item, Selected = tree.IsSelected(item)});
                }
                itemsRegion.ActivateView<BreadcrumbsMenu.Item.View, MenuItemModel>(new MenuItemModel {ID = last, Selected = tree.IsSelected(last)});
            }
        }
    }
}
