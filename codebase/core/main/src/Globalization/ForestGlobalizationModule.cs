using System;
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
using Axle.Resources.Binding;
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
        private readonly ResourceManager _resourceManager;
        private readonly ForestGlobalizationConfig _config;
        private readonly ILogger _logger;
        private readonly IDocumentBinder _binder;
        private readonly ICacheManager _cacheManager;

        public ForestGlobalizationModule(ResourceManager resourceManager, ForestGlobalizationConfig config, ILogger logger)
        {
            _resourceManager = resourceManager;
            _config = config;
            _logger = logger;
            _binder = new DefaultDocumentBinder(new GlobalizationObjectProvider(), new DefaultBindingConverter());
            _cacheManager = new SimpleCacheManager();
        }
        public ForestGlobalizationModule(ResourceManager resourceManager, ILogger logger) 
            : this(resourceManager, new ForestGlobalizationConfig(), logger) { }

        [ModuleTerminate]
        internal void OnTerminate()
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
            
            var textDocument = new ResourceDocumentRoot(_resourceManager, node.Name);
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