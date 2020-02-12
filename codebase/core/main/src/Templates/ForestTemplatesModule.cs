using System.Linq;
using Axle.Modularity;
using Axle.References;
using Axle.Resources;
using Axle.Resources.Extraction;
using Axle.Resources.Xml.Extraction;
using Forest.Templates.Xml;

namespace Forest.Templates
{
    [Module]
    internal sealed class ForestTemplatesModule : 
        IForestTemplateMarshallerConfigurer, 
        IForestTemplateMarshallerRegistry,
        IForestTemplateExtractorRegistry
    {
        private static string[] ForestBundleNames => new[] {ResourceTemplateProvider.BundleName, ResourceTemplateProvider.OldBundleName};

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
        }

        [ModuleDependencyInitialized]
        internal void ModuleDependencyInitialized(IForestTemplateMarshallerConfigurer cfg) => cfg.RegisterMarshallers(this);
        
        [ModuleDependencyInitialized]
        internal void ModuleDependencyInitialized(IForestTemplateExtractorConfigurer cfg) => cfg.RegisterTemplateExtractors(this);

        public void RegisterMarshallers(IForestTemplateMarshallerRegistry registry)
        {
            registry.Register(new ForestTemplateMarshaller(new XmlTemplateReader(), "xml", new XDocumentExtractor()));
        }

        public IForestTemplateMarshallerRegistry Register(IForestTemplateMarshaller marshaller)
        {
            var uriParser = new Axle.Conversion.Parsing.UriParser();
            var marshallingExtractor = new ForestTemplateExtractor(marshaller);
            var bundleNames = ForestBundleNames.Select(
                    b =>
                    {
                        var fb = $"{b}/{marshallingExtractor.Extension}";
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
                    .Register(uriParser.Parse($"./{bundleInfo.DefaultBundle}"))
                    .Register(uriParser.Parse($"./{bundleInfo.SpecificBundle}"))
                    .Extractors.Register(marshallingExtractor.ToExtractorList());
            }

            return this;
        }
        
        IForestTemplateExtractorRegistry IForestTemplateExtractorRegistry.Register(IResourceExtractor extractor)
        {
            var bundleNames = ForestBundleNames;
            foreach (var bundle in bundleNames)
            {
                _resourceManager.Bundles.Configure(bundle).Extractors.Register(extractor);
            }
            return this;
        }
    }

    internal sealed class PathTemplateExtractor : AbstractResourceExtractor
    {
        protected override Nullsafe<ResourceInfo> DoExtract(ResourceContext context, string name)
        {
            return base.DoExtract(context, name);
        }
    }
}