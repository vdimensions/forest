using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest
{
    [Requires(typeof(ForestViewRegistry))]
    [ReportsTo(typeof(ForestModule))]
    public interface IForestViewRegistryListener
    {
        void OnViewRegistered(IViewDescriptor viewDescriptor);
    }
}