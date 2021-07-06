using Axle.Application.Services;
using Axle.Modularity;
using Axle.Resources.Bundling;
using Forest.ComponentModel;

namespace Forest.Globalization.Bundling
{
    [Module]
    [Requires(typeof(ServiceRegistry))]
    internal sealed class ForestBundleConfigModule : ServiceGroup<ForestBundleConfigModule, IForestBundleConfigurer>
    {
        private readonly ServiceRegistry _serviceRegistry;
        
        public ForestBundleConfigModule(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
            _serviceRegistry = serviceRegistry;
        }

        public void ConfigureViewResourceBundle(IConfigurableBundleContent bundle, IForestViewDescriptor viewDescriptor)
        {
            foreach (var configurer in _serviceRegistry)
            {
                configurer.ConfigureViewResourceBundle(bundle, viewDescriptor);
            }
        }
    }
}