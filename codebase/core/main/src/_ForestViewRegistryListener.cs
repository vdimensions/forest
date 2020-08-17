using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest
{
    [Requires(typeof(ForestViewRegistry))]
    internal interface _ForestViewRegistryListener
    {
        void OnViewRegistered(IViewDescriptor viewDescriptor);
    }
}