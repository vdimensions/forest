using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;

namespace Forest.UI.Navigation.Breadcrumbs
{
    internal static class BreadcrumbsMenuNavigableItem
    {
        private const string Name = "BreadcrumbsMenuNavigableItem";

        private static class Commands
        {
            internal const string Navigate = "Navigate";
        }

        [View(Name)]
        internal class View : BreadcrumbsMenuItem.View
        {
            public View(NavigationNode model) : base(model) { }

            [Command(Commands.Navigate)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal static Location Navigate(NavigationNode model) => Location.Create(model.Path);
        }
    }
}