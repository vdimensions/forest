using System.Diagnostics.CodeAnalysis;

namespace Forest.Forms.Menus.Navigation
{
    public static partial class NavigationMenu
    {
        public static class Item
        {
            private const string Name = "ForestNavigationItem";

            public class View : LogicalView<MenuItemModel>
            {
                protected View(MenuItemModel model) : base(model)
                {
                }
            }
        }
        
        public static class NavigableItem
        {
            private const string Name = "ForestNavigationNavigableItem";
            
            private static class Commands
            {
                internal const string Navigate = "Navigate";
            }
            
            [View(Name)]
            public sealed class NavigableItemView : NavigationMenu.Item.View
            {
                public NavigableItemView(MenuItemModel model) : base(model)
                {
                }

                [Command(Commands.Navigate)]
                [SuppressMessage("ReSharper", "UnusedMember.Global")]
                internal void Navigate()
                {
                    Publish(new Messages.SelectNavigationItem(Model.ID));
                }
            }
        }
    }
}