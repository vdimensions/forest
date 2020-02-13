using System.Collections.Generic;
using System.Linq;
using Axle.References;
using Axle.Resources;
using Axle.Resources.Extraction;

namespace Forest.Templates
{
    internal sealed class ForestTemplateExtractor : AbstractResourceExtractor
    {
        private readonly IForestTemplateMarshaller _marshaller;

        public ForestTemplateExtractor(IForestTemplateMarshaller marshaller)
        {
            _marshaller = marshaller;
        }

        protected override Nullsafe<ResourceInfo> DoExtract(ResourceContext context, string name)
        {
            var baseResource = context.ExtractionChain.Extract($"{name}.{Extension}");
            if (baseResource.HasValue)
            {
                var t = _marshaller.Unmarshal(name, baseResource.Value);
                if (t != null)
                {
                    return Nullsafe<ResourceInfo>.Some(new ForestTemplateResourceInfo(name, context.Culture, baseResource.Value, t));
                }
            }
            return Nullsafe<ResourceInfo>.None;
        }

        internal IEnumerable<IResourceExtractor> ToExtractorList() => new[] {this}.Union(_marshaller.ChainedExtractors);

        public string Extension => _marshaller.Extension;
    }
}