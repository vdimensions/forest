using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Axle.Resources;

namespace Forest.Templates
{
    [Obsolete("Merge with ForestTemplatesModule class")]
    internal sealed class ResourceTemplateProvider : ITemplateProvider
    {
        internal const string BundleName = "Forest";
        [Obsolete]
        internal const string OldBundleName = "ForestTemplates";

        private readonly ResourceManager _resourceManager;
        private readonly IList<string> _bundles = new List<string>();
        private readonly ISet<string> _assemblies = new HashSet<string>(StringComparer.Ordinal);

        public ResourceTemplateProvider(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        internal void AddBundle(string bundle)
        {
            _bundles.Add(bundle);
        }

        internal void RegisterAssemblySource(Assembly asm)
        {
            var assemblyName = asm.GetName().Name;
            if (_assemblies.Add(assemblyName))
            {
                var uriParser = new Axle.Conversion.Parsing.UriParser();
                foreach (var bundle in _bundles)
                {
                    _resourceManager.Bundles
                        .Configure(bundle)
                        .Register(uriParser.Parse(string.Format("assembly://{0}/{1}", assemblyName, bundle)));
                }
            }
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
                if (template.HasValue)
                {
                    return template.Value.Resolve<Template>();
                }
                index = index + 1;
            }
        }

        public Template Load(string name)
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