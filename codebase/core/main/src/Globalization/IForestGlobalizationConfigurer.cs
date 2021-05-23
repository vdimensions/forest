using Axle.Resources.Bundling;
using Forest.ComponentModel;

namespace Forest.Globalization
{
    [RequiresForestGlobalization]
    public interface IForestGlobalizationConfigurer
    {
        void ConfigureViewResourceBundle(IConfigurableBundleContent bundle, IForestViewDescriptor viewDescriptor);
    }
}