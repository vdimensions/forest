using System.Diagnostics.CodeAnalysis;

namespace Forest.Forms.Menus.Navigation
{
    public static partial class BreadcrumbsMenu
    {
        public static class Item
        {
            private const string Name = "ForestBreadcrumbsItem";

            public class View : LogicalView<MenuItemModel>
            {
                protected View(MenuItemModel model) : base(model)
                {
                }
            }
        }
        
        public static class NavigableItem
        {
            private const string Name = "ForestBreadcrumbsNavigableItem";

            private static class Commands
            {
                internal const string Navigate = "Navigate";
            }

            [View(Name)]
            public class View : Item.View
            {
                public View(MenuItemModel model) : base(model) { }

                [Command(Commands.Navigate)]
                [SuppressMessage("ReSharper", "UnusedMember.Global")]
                internal void Navigate()
                {
                    Publish(new NavigationMenu.Messages.SelectNavigationItem(Model.ID));
                }
            }
        }
    }
}