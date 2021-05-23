using Axle.Resources.Bundling;
using Forest.ComponentModel;

namespace Forest.Globalization.Bundling
{
    [ForestBundleConfig(IsPublic = false)]
    public interface IForestBundleConfigurer
    {
        void ConfigureViewResourceBundle(IConfigurableBundleContent bundle, IForestViewDescriptor viewDescriptor);
    }
}