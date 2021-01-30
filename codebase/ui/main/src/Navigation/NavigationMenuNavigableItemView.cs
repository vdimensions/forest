using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;

namespace Forest.UI.Navigation
{
    [View(Name)]
    internal sealed class NavigationMenuNavigableItemView : NavigationMenuItemView
    {
        private const string Name = "NavigationMenuNavigableItem";
            
        private static class Commands
        {
            internal const string Navigate = "Navigate";
        }
            
        public NavigationMenuNavigableItemView(NavigationNode model) : base(model) { }

        [Command(Commands.Navigate)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static Location Navigate(NavigationNode model) => Location.Create(model.Path);
    }
}