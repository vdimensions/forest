using System.Collections.Generic;
using System.Linq;
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

        protected override ResourceInfo DoExtract(IResourceContext context, string name)
        {
            var ext = $".{Extension}";
            var resourceName = name.EndsWith(ext) ? name : $"{name}{ext}";
            var baseResource = context.Extract(resourceName);
            if (baseResource != null)
            {
                var t = _marshaller.Unmarshal(name, baseResource);
                if (t != null)
                {
                    return new ForestTemplateResourceInfo(name, context.Culture, baseResource, t);
                }
            }
            return null;
        }

        internal IEnumerable<IResourceExtractor> ToExtractorList() => (_marshaller.ChainedExtractors).Union(new[] {this});

        public string Extension => _marshaller.Extension;
    }
}