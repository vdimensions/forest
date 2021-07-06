using Axle.Application.Services;
using Axle.Modularity;

namespace Forest.Globalization.Bundling
{
    [Requires(typeof(ForestBundleConfigModule.ServiceRegistry))]
    [ProvidesFor(typeof(ForestBundleConfigModule))]
    internal sealed class ForestBundleConfigAttribute : AbstractServiceAttribute { }
}