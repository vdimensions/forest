using Axle.Modularity;

namespace Forest.Templates
{
    [Requires(typeof(ForestTemplatesModule))]
    public interface IForestTemplateExtractorConfigurer
    {
        void RegisterTemplateExtractors(IForestTemplateExtractorRegistry registry);
    }
}