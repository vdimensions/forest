using System.Collections.Generic;
using Axle.Resources;
using Axle.Resources.Extraction;

namespace Forest.Templates
{
    public interface IForestTemplateMarshaller
    {
        Template Unmarshal(string name, ResourceInfo resource);
        IEnumerable<IResourceExtractor> ChainedExtractors { get; }
        string Extension { get; }
    }
}
