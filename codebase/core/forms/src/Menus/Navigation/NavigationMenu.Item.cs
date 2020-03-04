using System.Diagnostics.CodeAnalysis;

namespace Forest.Forms.Menus.Navigation
{
    public static partial class NavigationMenu
    {
        internal static class Item
        {
            private const string Name = "NavigationMenuItem";

            [View(Name)]
            internal class View : LogicalView<MenuItemModel>
            {
                protected View(MenuItemModel model) : base(model) { }
            }
        }
        
        internal static class NavigableItem
        {
            private const string Name = "NavigationMenuNavigableItem";
            
            private static class Commands
            {
                internal const string Navigate = "Navigate";
            }
            
            [View(Name)]
            internal sealed class View : NavigationMenu.Item.View
            {
                public View(MenuItemModel model) : base(model) { }

                [Command(Commands.Navigate)]
                [SuppressMessage("ReSharper", "UnusedMember.Global")]
                internal void Navigate()
                {
                    Engine.Navigate(Model.ID);
                }
            }
        }
    }
}