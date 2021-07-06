using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;

namespace Forest.UI.Navigation.Breadcrumbs
{
    [View(Name)]
    internal class BreadcrumbsMenuNavigableItemView : BreadcrumbsMenuItemView
    {
        private const string Name = "BreadcrumbsMenuNavigableItem";

        private static class Commands
        {
            internal const string Navigate = "Navigate";
        }

        public BreadcrumbsMenuNavigableItemView(NavigationNode model) : base(model) { }

        [Command(Commands.Navigate)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static Location Navigate(NavigationNode model) => Location.Create(model.Path);
    }
}