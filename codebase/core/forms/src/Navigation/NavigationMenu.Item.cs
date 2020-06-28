using System.Diagnostics.CodeAnalysis;

namespace Forest.Forms.Navigation
{
    public static partial class NavigationMenu
    {
        internal static class Item
        {
            private const string Name = "NavigationMenuItem";

            [View(Name)]
            internal class View : LogicalView<NavigationNode>
            {
                protected View(NavigationNode model) : base(model) { }
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
            internal sealed class View : Forms.Navigation.NavigationMenu.Item.View
            {
                public View(NavigationNode model) : base(model) { }

                [Command(Commands.Navigate)]
                [SuppressMessage("ReSharper", "UnusedMember.Global")]
                internal void Navigate()
                {
                    Engine.Navigate(Model.Path);
                }
            }
        }
    }
}