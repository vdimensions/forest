using Axle.Modularity;
using Forest.UI.Containers.TabStrip;
using Forest.UI.Dialogs;
using Forest.UI.Navigation;
using Forest.UI.Navigation.Breadcrumbs;

namespace Forest.UI
{
    [Module]
    [Requires(typeof(DialogSystem.Module))]
    [Requires(typeof(NavigationMenu.Module))]
    [Requires(typeof(BreadcrumbsMenu.Module))]
    [Requires(typeof(TabStripModule))]
    internal sealed class ForestUIModule { }
}