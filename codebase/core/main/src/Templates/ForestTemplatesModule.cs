using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Axle.DependencyInjection;
using Axle.Modularity;
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
        IForestTemplateExtractorRegistry,
        ITemplateProvider
    {
        internal const string BundleName = "Forest";
        
        private static string[] ForestBundleNames => new[] {BundleName};
        
        private readonly IList<string> _bundles = new List<string>();
        private readonly LinkedList<IResourceExtractor> _templateSourceExtractors = new LinkedList<IResourceExtractor>();
        private readonly IList<ForestTemplateExtractor> _marshallingExtractors = new List<ForestTemplateExtractor>();
        private readonly ISet<string> _assemblies = new HashSet<string>(StringComparer.Ordinal);

        private ResourceManager _resourceManager;
        

        public ForestTemplatesModule()
        {
            _resourceManager = new DefaultResourceManager();
        }

        [ModuleInit]
        internal void Init(IDependencyExporter e)
        {
            ModuleDependencyInitialized(this);
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

        private void InitResourceManager()
        {
            _bundles.Clear();
            var resourceManager = new DefaultResourceManager();
            var uriParser = new Axle.Conversion.Parsing.UriParser();
            foreach (var marshallingExtractor in _marshallingExtractors)
            {
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
                    var bundle = bundleInfo.DefaultBundle;
                    _bundles.Add(bundle);
                    _bundles.Add(bundleInfo.SpecificBundle);
                    var bundleContent = resourceManager.Bundles.Configure(bundle);
                    bundleContent
                        .Register(uriParser.Parse($"./{bundleInfo.DefaultBundle}"))
                        .Extractors
                        .Register(_templateSourceExtractors)
                        .Register(marshallingExtractor.ToExtractorList());
                    bundleContent
                        .Register(uriParser.Parse($"./{bundleInfo.SpecificBundle}"))
                        .Extractors
                        .Register(_templateSourceExtractors)
                        .Register(marshallingExtractor.ToExtractorList());
                    foreach (var assemblyName in _assemblies)
                    {
                        bundleContent.Register(uriParser.Parse($"assembly://{assemblyName}/{bundle}"));
                    }
                }
            }

            _resourceManager = resourceManager;
        }
        
        public void RegisterAssemblySource(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            _assemblies.Add(assemblyName);
            InitResourceManager();
        }

        public IForestTemplateMarshallerRegistry Register(IForestTemplateMarshaller marshaller)
        {
            var marshallingExtractor = new ForestTemplateExtractor(marshaller);
            _marshallingExtractors.Add(marshallingExtractor);
            InitResourceManager();
            return this;
        }
        
        IForestTemplateExtractorRegistry IForestTemplateExtractorRegistry.Register(IResourceExtractor extractor)
        {
            _templateSourceExtractors.AddFirst(extractor);
            InitResourceManager();
            return this;
        }
        
        private Template Load(IList<string> bundles, string name, int index)
        {
            while (true)
            {
                if (index >= bundles.Count)
                {
                    return null;
                }

                var bundle = bundles[index];
                var template = _resourceManager.Load(bundle, name, CultureInfo.InvariantCulture);
                if (template != null)
                {
                    return template.Resolve<Template>();
                }
                index = index + 1;
            }
        }

        Template ITemplateProvider.Load(string name)
        {
            var result = Load(_bundles, name, 0);
            if (result != null)
            {
                return result;
            }
            throw new ResourceNotFoundException(name, BundleName, CultureInfo.InvariantCulture);
        }
    }
}