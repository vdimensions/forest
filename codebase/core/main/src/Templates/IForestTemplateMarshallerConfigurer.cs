using Axle.Modularity;

namespace Forest.Templates
{
    [Requires(typeof(ForestTemplatesModule))]
    public interface IForestTemplateMarshallerConfigurer
    {
        void RegisterMarshallers(IForestTemplateMarshallerRegistry registry);
    }
}