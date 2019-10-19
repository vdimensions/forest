using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest
{
    [Requires(typeof(ForestViewRegistry))]
    [UtilizedBy(typeof(ForestModule))]
    public interface IForestViewProvider
    {
        void RegisterViews(IViewRegistry registry);
    }
}