using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Axle.Caching;
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
using Axle.Resources.Bundling;
using Axle.Text.Documents;
using Axle.Text.Documents.Binding;
using Forest.ComponentModel;
using Forest.Dom;
using Forest.Globalization.Configuration;

namespace Forest.Globalization
{
    [Module]
    [RequiresResources]
    [ModuleConfigSection(typeof(ForestGlobalizationConfig), "Forest.Globalization")]
    internal sealed class ForestGlobalizationModule : IDomProcessor, _ForestViewRegistryListener
    {
        private sealed class ResourceDocumentRoot : ITextDocumentRoot
        {
            private readonly ResourceManager _resourceManager;
            private readonly string _bundle;
            private readonly string _prefix;

            public ResourceDocumentRoot(ResourceManager resourceManager, string bundle, string prefix, string key, ITextDocumentObject parent)
            {
                _resourceManager = resourceManager;
                _bundle = bundle;
                _prefix = prefix;
                Key = key;
                Parent = parent;
            }
            
            public IEnumerable<ITextDocumentNode> GetChildren() => Enumerable.Empty<ITextDocumentNode>();

            public IEnumerable<ITextDocumentNode> GetValues(string name)
            {
                var prefixOrKey = _prefix.Length == 0 ? name : $"{_prefix}.{name}";
                var resource = _resourceManager.Load<string>(_bundle, prefixOrKey, CultureInfo.CurrentUICulture);
                if (!string.IsNullOrEmpty(resource))
                {
                    yield return new TextDocumentValue(name, this, resource);
                }
                else
                {
                    yield return new ResourceDocumentRoot(_resourceManager, _bundle, prefixOrKey, name, this);
                }
            }

            public ITextDocumentObject Parent { get; }
            public string Key { get; }
            public IEqualityComparer<string> KeyComparer => StringComparer.Ordinal;
        }
        
        private readonly IDocumentBinder _binder = new DefaultDocumentBinder(new GlobalizationObjectProvider(), new DefaultBindingConverter());

        private readonly ResourceManager _resourceManager;
        private readonly ForestGlobalizationConfig _config;
        private readonly ILogger _logger;
        private readonly ICacheManager _cacheManager;

        public ForestGlobalizationModule(ResourceManager resourceManager, ForestGlobalizationConfig config, ILogger logger)
        {
            _resourceManager = resourceManager;
            _config = config;
            _logger = logger;
            _cacheManager = new SimpleCacheManager();
        }
        public ForestGlobalizationModule(ResourceManager resourceManager, ILogger logger) 
            : this(resourceManager, new ForestGlobalizationConfig(), logger) { }

        [ModuleTerminate]
        internal void OnTerminate()
        {
            _cacheManager.Dispose();
        }
        
        private ITextDocumentRoot GetTextDocument(string viewName)
        {
            return new ResourceDocumentRoot(_resourceManager, viewName, string.Empty, string.Empty, null);
        }

        private bool IsSafeFromSideEffects(Type type)
        {
            return _cacheManager
                .GetCache(nameof(IsSafeFromSideEffects))
                .GetOrAdd(type, ShallowCopy.IsSafeFromSideEffects);
        }

        private bool TryCloneObject(object obj, out object clone)
        {
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

            if (!LocalizedAttribute.IsLocalized(obj.GetType()))
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
            
            var textDocument = GetTextDocument(node.Name);
            if (textDocument == null)
            {
                _logger.Warn("Could not locate localization bundle for view '{0}'", node.Name);
                return node;
            }
            
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
                _logger.Warn(
                    "Unable to create a cloned model object to use for localization: view '{0}'.", 
                    node.Name);
                newModel = node.Model;
            }
            if (ReferenceEquals(newModel, node.Model) && ReferenceEquals(newCommands, node.Commands))
            {
                return node;
            }
            return new DomNode(node.InstanceID, node.Name, node.Region, newModel, node.Parent, node.Regions, newCommands, node.Revision);
        }

        public void OnViewRegistered(IForestViewDescriptor viewDescriptor)
        {
            var viewType = viewDescriptor.ViewType;
            var uriParser = new Axle.Conversion.Parsing.UriParser();
            #if NETSTANDARD || NET45_OR_NEWER
            var asm = viewType.GetTypeInfo().Assembly;
            #else
            var asm = viewType.Assembly;
            #endif
            var viewBundle = _resourceManager.Bundles.Configure(viewDescriptor.Name);
            if (_config.AutoRegisterLocalizationBundles)
            {
                viewBundle
                    .Register(uriParser.Parse($"resx://{asm.GetName().Name}/Resources/{viewDescriptor.Name}/"))
                    .Register(asm, $"Resources.properties/{viewDescriptor.Name}/")
                    .Register(asm, $"Resources.yaml/{viewDescriptor.Name}/")
                    .Register(asm, $"Resources.yml/{viewDescriptor.Name}/")
                    ;
            }
        }
    }
}