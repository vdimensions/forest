using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest.UI.Containers.TabStrip
{
    [Module]
    internal sealed class TabStripModule : IForestViewProvider
    {
        public void RegisterViews(IForestViewRegistry registry)
        {
            registry
                .Register<TabView>()
                .Register<TabStripView>();
        }
    }
}
