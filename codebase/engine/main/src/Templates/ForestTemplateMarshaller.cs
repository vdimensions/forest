using System.Collections.Generic;
using Axle.Resources;
using Axle.Resources.Extraction;

namespace Forest.Templates
{
    internal sealed class ForestTemplateMarshaller : IForestTemplateMarshaller
    {
        private readonly TemplateReader _reader;

        public ForestTemplateMarshaller(TemplateReader reader, string extension, params IResourceExtractor[] chainedExtractors)
        {
            _reader = reader;
            ChainedExtractors = chainedExtractors;
            Extension = extension;
        }

        Template IForestTemplateMarshaller.Unmarshal(string name, ResourceInfo resource)
        {
            using (var stream = resource.Open())
            {
                return _reader.Read(name, stream);
            }
        }

        public IEnumerable<IResourceExtractor> ChainedExtractors { get; }
        public string Extension { get; }
    }
}