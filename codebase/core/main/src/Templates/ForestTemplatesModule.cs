using System.Linq;
using Axle.Modularity;
using Axle.Resources;
using Axle.Resources.Extraction;
using Axle.Resources.Xml.Extraction;
using Forest.Templates.Xml;

namespace Forest.Templates
{
    [Module]
    internal sealed class ForestTemplatesModule : IForestTemplateMarshallerConfigurer, IForestTemplateMarshallerRegistry
    {
        private readonly ResourceManager _resourceManager;
        private readonly ResourceTemplateProvider _templateProvider;

        public ForestTemplatesModule()
        {
            _templateProvider = new ResourceTemplateProvider(_resourceManager = new DefaultResourceManager());
        }

        [ModuleInit]
        internal void Init(ModuleExporter e)
        {
            ModuleDependencyInitialized(this);
            e.Export(_templateProvider);
        }

        [ModuleTerminate]
        internal void Terminate()
        {
            ModuleDependencyTerminated(this);
        }

        [ModuleDependencyInitialized]
        internal void ModuleDependencyInitialized(IForestTemplateMarshallerConfigurer cfg) => cfg.RegisterMarshallers(this);

        [ModuleDependencyTerminated]
        internal void ModuleDependencyTerminated(IForestTemplateMarshallerConfigurer cfg) { }

        public void RegisterMarshallers(IForestTemplateMarshallerRegistry registry)
        {
            registry.Register(new ForestTemplateMarshaller(new XmlTemplateReader(), "xml", new XDocumentExtractor()));
        }

        public IForestTemplateMarshallerRegistry Register(IForestTemplateMarshaller marshaller)
        {
            var uriParser = new Axle.Conversion.Parsing.UriParser();
            var marshallingExtractor = new ForestTemplateExtractor(marshaller);
            var bundleNames = 
                new[] { ResourceTemplateProvider.BundleName, ResourceTemplateProvider.OldBundleName }
                    .Select(
                        b =>
                        {
                            var fb = string.Format("{0}/{1}", b, marshallingExtractor.Extension);
                            return new
                            {
                                DefaultBundle = b,
                                SpecificBundle = fb
                            };
                        });
            foreach (var bundleInfo in bundleNames)
            {
                _templateProvider.AddBundle(bundleInfo.DefaultBundle);
                _templateProvider.AddBundle(bundleInfo.SpecificBundle);
                _resourceManager.Bundles
                    .Configure(bundleInfo.DefaultBundle)
                    .Register(uriParser.Parse(string.Format("./{0}", bundleInfo.DefaultBundle)))
                    .Register(uriParser.Parse(string.Format("./{0}", bundleInfo.SpecificBundle)))
                    .Extractors.Register(marshallingExtractor.ToExtractorList());
            }

            return this;
        }
    }
}