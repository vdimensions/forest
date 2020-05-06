using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest
{
    [Requires(typeof(ForestViewRegistry))]
    internal interface _ForestViewProvider
    {
        void RegisterViews(IViewRegistry registry);
    }
}