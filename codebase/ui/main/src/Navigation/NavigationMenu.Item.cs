using System.Diagnostics.CodeAnalysis;
using Forest.Globalization;
using Forest.Navigation;

namespace Forest.UI.Navigation
{
    public static partial class NavigationMenu
    {
        internal static class Item
        {
            private const string Name = "NavigationMenuItem";

            [View(Name)]
            internal class View : LogicalView<NavigationNode>, ISupportsCustomGlobalizationKey<NavigationNode>
            {
                private string ObtainGlobalizationKey(NavigationNode model)
                {
                    return $"{Name}.{model.Path.Replace("/", ".")}";
                }

                string ISupportsCustomGlobalizationKey<NavigationNode>.ObtainGlobalizationKey(NavigationNode model)
                {
                    return ObtainGlobalizationKey(model);
                }

                string ISupportsCustomGlobalizationKey.ObtainGlobalizationKey(object model)
                {
                    if (model is NavigationNode navigationNode)
                    {
                        return ObtainGlobalizationKey(navigationNode);
                    }
                    return null;
                }
                
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
            internal sealed class View : NavigationMenu.Item.View
            {
                public View(NavigationNode model) : base(model) { }

                [Command(Commands.Navigate)]
                [SuppressMessage("ReSharper", "UnusedMember.Global")]
                internal Location Navigate() => Location.Create(Model.Path);
            }
        }
    }
}