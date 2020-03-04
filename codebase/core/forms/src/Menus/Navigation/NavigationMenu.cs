using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Axle.Verification;

namespace Forest.Forms.Menus.Navigation
{
    public static partial class NavigationMenu
    {
        private const string Name = "NavigationMenu";

        private static class Regions
        {
            public const string Items = "Items";
        }

        internal abstract class AbstractView : LogicalView
        {
            [Subscription(NavigationSystem.Messages.Topic)]
            protected abstract void OnNavigationTreeChanged(NavigationTree tree);
        }

        [View(Name)]
        internal sealed class View : AbstractView
        {
            public override void Load()
            {
                Engine.RegisterSystemView<NavigationSystem.View>();
                base.Load();
            }

            protected override void OnNavigationTreeChanged(NavigationTree tree)
            {
                WithRegion(Regions.Items, itemsRegion =>
                {
                    itemsRegion.Clear();
                    var topLevel = tree.TopLevelNodes
                            .Select(x => new MenuItemModel { ID = x, Selected = tree.IsSelected(x) });
                    foreach (var item in topLevel)
                    {
                        if (item.Selected)
                        {
                            itemsRegion.ActivateView<Item.View, MenuItemModel>(item);
                        }
                        else
                        {
                            itemsRegion.ActivateView<NavigableItem.View, MenuItemModel>(item);
                        }
                    }
                });
            }
        }
    }
}
