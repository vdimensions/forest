using Axle.Modularity;
using Forest.Forms.Controls;
using Forest.Forms.Controls.Dialogs;
using Forest.Forms.Navigation;
using Forest.Forms.Navigation.Breadcrumbs;

namespace Forest.Forms
{
    [Module]
    [Requires(typeof(DialogSystem.Module))]
    [Requires(typeof(NavigationMenu.Module))]
    [Requires(typeof(BreadcrumbsMenu.Module))]
    [Requires(typeof(TabStrip.Module))]
    internal sealed class ForestFormsModule //: IForestFormsManagerProvider
    {
    }
}
