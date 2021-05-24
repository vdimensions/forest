using System;
using System.Globalization;
using Axle.Caching;
using Axle.Extensions.String;
using Axle.Globalization;
#if NETSTANDARD || NET45_OR_NEWER
using System.Reflection;
#endif
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.IO;
using System.Runtime.Serialization;
using Axle.IO.Serialization;
#endif
using Axle.Logging;
using Axle.Modularity;
using Axle.References;
using Axle.Resources;
using Axle.Resources.Binding;
using Axle.Resources.Bundling;
using Axle.Resources.Properties.Extraction;
using Axle.Resources.ResX.Extraction;
using Axle.Text.Documents;
using Axle.Text.Documents.Binding;
using Forest.ComponentModel;
using Forest.Dom;
using Forest.Globalization.Bundling;
using Forest.Globalization.Configuration;

namespace Forest.Globalization
{
    [Module]
    [RequiresResources]
    [Requires(typeof(ForestBundleConfigModule))]
    [ModuleConfigSection(typeof(ForestGlobalizationConfig), "Forest.Globalization")]
    internal sealed class ForestGlobalizationModule 
        : IDomProcessor, 
          _ForestViewRegistryListener,
          IDisposable
    {
        private static CultureScope CreateCultureScope(string cultureName, ILogger logger)
        {
            if (string.IsNullOrEmpty(cultureName))
            {
                return null;
            }
            var defaultLocale = CultureInfo.CurrentUICulture;
            try
            {
                return CultureScope.Create(cultureName);
            }
            catch (Exception e)
            {
                logger.Warn(
                    $"An error occurred while trying to create CultureInfo object from language code '{cultureName}'. The default system locale '{defaultLocale}' will be used instead.", e);
                return null;
            }
        }
        
        private readonly ResourceManager _resourceManager;
        private readonly ForestGlobalizationConfig _config;
        private readonly ILogger _logger;
        private readonly IDocumentBinder _binder;
        private readonly ICacheManager _cacheManager;
        private readonly ForestBundleConfigModule _bundleConfig;
        private readonly IForestViewRegistry _viewRegistry;

        public ForestGlobalizationModule(ResourceManager resourceManager,
            ForestBundleConfigModule bundleConfig,
            IForestViewRegistry viewRegistry,
            ForestGlobalizationConfig config, 
            ILogger logger)
        {
            _resourceManager = resourceManager;
            _bundleConfig = bundleConfig;
            _viewRegistry = viewRegistry;
            _config = config;
            _logger = logger;
            _binder = new DefaultDocumentBinder(new GlobalizationObjectProvider(), new DefaultBindingConverter());
            _cacheManager = new SimpleCacheManager();
        }
        public ForestGlobalizationModule(
                ResourceManager resourceManager,
                ForestBundleConfigModule bundleConfig, 
                IForestViewRegistry viewRegistry, 
                ILogger logger) 
            : this(resourceManager, bundleConfig, viewRegistry, new ForestGlobalizationConfig(), logger) { }


        void IDisposable.Dispose()
        {
            _cacheManager.Dispose();
        }
        
        private bool IsSafeFromSideEffects(Type type)
        {
            return _cacheManager
                .GetCache(nameof(IsSafeFromSideEffects))
                .GetOrAdd(type, ShallowCopy.IsSafeFromSideEffects);
        }

        private bool IsLocalized(Type type)
        {
            return _cacheManager
                .GetCache(nameof(LocalizedAttribute.IsLocalized))
                .GetOrAdd(type, LocalizedAttribute.IsLocalized);
        }

        private bool TryCloneObject(object obj, out object clone)
        {
            if (obj is IGlobalizationCloneable globalizationCloneable)
            {
                clone = globalizationCloneable.Clone();
                return true;
            }
            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            if (obj is ICloneable cloneable)
            {
                clone = cloneable.Clone();
                return true;
            }
            #endif
            if (IsSafeFromSideEffects(obj.GetType()))
            {
                clone = ShallowCopy.Create(obj);
                return true;
            }
            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            var stream = new MemoryStream();
            try
            {
                var serializer = new BinarySerializer();
                serializer.Serialize(obj, stream);
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                clone = serializer.Deserialize(stream, obj.GetType());
                return true;
            }
            catch (SerializationException)
            {
                clone = null;
                return false;
            }
            finally
            {
                stream.Dispose();
            }
            #else
            clone = null;
            return false;
            #endif
        }

        private bool TryLocalize(ITextDocumentRoot localizationSource, object obj, out object localizedObj)
        {
            localizedObj = null;
            if (obj == null)
            {
                return false;
            }

            if (!IsLocalized(obj.GetType()))
            {
                return false;
            }

            if (!TryCloneObject(obj, out var clone))
            {
                return false;
            }
            
            localizedObj = _binder.Bind(localizationSource, clone);
            return true;
        }

        DomNode IDomProcessor.ProcessNode(DomNode node, bool isNodeUpdated)
        {
            if (!isNodeUpdated)
            {
                return node;
            }

            var scope = CreateCultureScope(_config.DisplayLanguage, _logger);
            try
            {
                var textDocument = ResolveTextDocument(node);
                var newCommands = node.Commands;
                var cmdKeys = newCommands.Keys;
                foreach (var commandName in cmdKeys)
                {
                    var cmd = newCommands[commandName];
                    if (TryLocalize(new TextDocumentSubset(textDocument, $"Commands.{commandName}"), cmd, out var cmdClone))
                    {
                        newCommands = newCommands.Remove(commandName).Add(commandName, (ICommandModel) cmdClone);
                    }
                    else
                    {
                        _logger.Warn(
                            "Unable to create a cloned command object to use for localization: command '{0}' for view '{1}'.", 
                            commandName, 
                            node.Name);   
                    }
                }

                if (!TryLocalize(new TextDocumentSubset(textDocument, "Model"), node.Model, out var newModel))
                {
                    if (node.Model != null)
                    {
                        _logger.Warn("Unable to clone the view model to use for localization: view '{0}'.",  node.Name);
                    }
                    newModel = node.Model;
                }
                if (ReferenceEquals(newModel, node.Model) && ReferenceEquals(newCommands, node.Commands))
                {
                    return node;
                }
                return new DomNode(
                    node.InstanceID, 
                    node.Name, 
                    node.Region, 
                    newModel, 
                    node.Parent, 
                    node.Regions, 
                    newCommands, 
                    node.ResourceBundle, 
                    node.Revision);
            }
            finally
            {
                scope?.Dispose();
            }
        }

        private ITextDocumentObject ResolveTextDocument(DomNode node)
        {
            if (string.IsNullOrEmpty(node.ResourceBundle))
            {
                return new ResourceDocumentRoot(_resourceManager, node.Name);
            }
            else
            {
                var bundleName = node.ResourceBundle.TrimStart($"{node.Name}.", StringComparison.Ordinal);
                RegisterViewBundle(bundleName, _viewRegistry.Describe(node.Name));
                ITextDocumentObject textDocument = new ResourceDocumentRoot(_resourceManager, bundleName);
                return new TextDocumentSubset(textDocument, node.Name);
            }
        }

        public void OnViewRegistered(IForestViewDescriptor viewDescriptor)
        {
            if (_config.AutoRegisterLocalizationBundles)
            {
                RegisterViewBundle(viewDescriptor.Name, viewDescriptor);
            }
        }

        private void RegisterViewBundle(string bundleName, IForestViewDescriptor viewDescriptor)
        {
            if (_resourceManager.Bundles.Contains(bundleName))
            {
                return;
            }
            var viewBundle = _resourceManager.Bundles.Configure(bundleName);
            ConfigureViewResourceBundle(viewBundle, viewDescriptor);
        }

        private void ConfigureViewResourceBundle(IConfigurableBundleContent bundle, IForestViewDescriptor viewDescriptor)
        {
            if (viewDescriptor != null)
            {
                #if NETSTANDARD || NET45_OR_NEWER
                var asm = viewDescriptor.ViewType.GetTypeInfo().Assembly;
                #else
                var asm = viewDescriptor.ViewType.Assembly;
                #endif
                var uriParser = new Axle.Text.Parsing.UriParser();
                var propertiesDir = "Properties";

                bundle
                    .Register(asm, $"{propertiesDir}/")
                    .Register(uriParser.Parse($"resx://{asm.GetName().Name}/{propertiesDir}/{bundle.Bundle}/"))
                    ;
            }
            
            bundle.Extractors
                .Register(new PropertiesExtractor($"{bundle.Bundle}.properties"))
                .Register(new PropertiesExtractor($"Strings.properties/{bundle.Bundle}/"))
                .Register(new ResXResourceExtractor())
                ;
            _bundleConfig.ConfigureViewResourceBundle(bundle, viewDescriptor);
        }
    }
}