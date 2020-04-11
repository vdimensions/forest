using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest
{
    [Requires(typeof(ForestViewRegistry))]
    [ReportsTo(typeof(ForestModule))]
    public interface IForestViewProvider
    {
        void RegisterViews(IViewRegistry registry);
    }
}