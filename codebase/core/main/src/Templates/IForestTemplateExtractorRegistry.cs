using Axle.Resources.Extraction;

namespace Forest.Templates
{
    public interface IForestTemplateExtractorRegistry
    {
        IForestTemplateExtractorRegistry Register(IResourceExtractor extractor);
    }
}