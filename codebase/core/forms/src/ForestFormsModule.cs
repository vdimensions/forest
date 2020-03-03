using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Forms.Controls;
using Forest.Forms.Controls.Dialogs;
using Forest.Forms.Menus.Navigation;

namespace Forest.Forms
{
    [Module]
    [Requires(typeof(ForestDialogsModule))]
    [Requires(typeof(NavigationSystemModule))]
    internal sealed class ForestFormsModule : IForestViewProvider//, IForestFormsManagerProvider
    {
        public void RegisterViews(IViewRegistry registry)
        {
            registry
                .Register<TabStrip.Tab.View>()
                .Register<TabStrip.View>();
        }
    }
}
