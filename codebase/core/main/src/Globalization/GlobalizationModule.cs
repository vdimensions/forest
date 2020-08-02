using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Axle.Logging;
using Axle.Modularity;
using Axle.Resources;
using Axle.Text.Documents;
using Axle.Text.Documents.Binding;
using Forest.Dom;

namespace Forest.Globalization
{
    
    [Module]
    [RequiresResources]
    internal sealed class GlobalizationModule : IDomProcessor
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

            public IEnumerable<ITextDocumentNode> GetChildren(string name)
            {
                var prefixOrKey = $"{_prefix}.{name}";
                var resource = _resourceManager.Load<string>(_bundle, prefixOrKey, CultureInfo.CurrentUICulture);
                if (!string.IsNullOrEmpty(resource))
                {
                    yield return new TextDocumentValue(name, this, resource);
                }
                else
                {
                    // TODO: `prefixOrKey` and `name` are not correctly inferred
                    yield return new ResourceDocumentRoot(_resourceManager, _bundle, prefixOrKey, name, this);
                }
            }

            public ITextDocumentObject Parent { get; }
            public string Key { get; }
            public IEqualityComparer<string> KeyComparer => StringComparer.Ordinal;
        }
        
        private const string Bundle = "70938F73-29B6-4D19-ADD2-CB56D2373720";
        
        private readonly IBinder _binder = new DefaultBinder(new GlobalizationObjectProvider(), new DefaultBindingConverter());

        private readonly ResourceManager _resourceManager;
        private readonly ILogger _logger;

        public GlobalizationModule(ResourceManager resourceManager, ILogger logger)
        {
            _resourceManager = resourceManager;
            _logger = logger;
        }

        private ITextDocumentRoot GetTextDocument(string viewName)
        {
            return new ResourceDocumentRoot(_resourceManager, Bundle, viewName, string.Empty, null);
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
            
            var modelTextDocument = new TextDocumentSubset(textDocument, "Model");
            var newCommands = node.Commands;
            var cmdKeys = newCommands.Keys;
            foreach (var commandName in cmdKeys)
            {
                var cmd = newCommands[commandName];
                newCommands
                    .Remove(commandName)
                    .Add(
                        commandName, 
                        (ICommandModel) _binder.Bind(new TextDocumentSubset(textDocument, $"Commands.{commandName}"), cmd));
            }
            var newModel =  node.Model == null ? null : _binder.Bind(modelTextDocument, node.Model);
            return new DomNode(node.InstanceID, node.Name, node.Region, newModel, node.Parent, node.Regions, newCommands);
        }
    }
}