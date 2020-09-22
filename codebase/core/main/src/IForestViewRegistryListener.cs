using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest
{
    [Requires(typeof(ForestViewRegistry))]
    [ProvidesFor(typeof(ForestModule))]
    public interface IForestViewRegistryListener
    {
        void OnViewRegistered(IForestViewDescriptor viewDescriptor);
    }
}