using System.Linq;

namespace Forest.Forms.Menus.Navigation
{
    public static partial class BreadcrumbsMenu
    {
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
                    var selectedItems = tree.SelectedNodes
                        .Select(x => new MenuItemModel {ID = x, Selected = true})
                        .ToArray();
                    if (selectedItems.Length == 0)
                    {
                        return;
                    }
                    var last = selectedItems[selectedItems.Length - 1];
                    for (var i = 0; i < selectedItems.Length - 1; i++)
                    {
                        itemsRegion.ActivateView<BreadcrumbsMenu.NavigableItem.View, MenuItemModel>(selectedItems[i]);
                    }
                    itemsRegion.ActivateView<BreadcrumbsMenu.Item.View, MenuItemModel>(last);
                });
            }
        }
    }
}
