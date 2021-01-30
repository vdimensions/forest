using Axle.Modularity;
using Forest.ComponentModel;
using Forest.UI.Containers.TabStrip;
using Forest.UI.Dialogs;
using Forest.UI.Forms;
using Forest.UI.Navigation;
using Forest.UI.Navigation.Breadcrumbs;

namespace Forest.UI
{
    [Module]
    [Requires(typeof(FormFieldsModule))]
    internal sealed class ForestUIModule : IForestViewProvider
    {
        public void RegisterViews(IForestViewRegistry registry)
        {
            registry
                .Register<DialogSystem.View>()
                .Register<NavigationMenuView>()
                .Register<BreadcrumbsMenuView>()
                .Register<TabStripView>()
                ;
        }
    }
}