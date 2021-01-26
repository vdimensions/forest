using System;
using System.Collections.Generic;
using System.Linq;
using Axle.Collections.Immutable;
using Axle.Logging;
using Forest.ComponentModel;

namespace Forest.Dom
{
    internal sealed class ForestDomManager : IForestDomManager
    {
        private static ICommandModel CreateModel(IForestCommandDescriptor descriptor, ViewState viewState)
        {
            if (descriptor.TryResolveRedirect(viewState.Model, out var redirect))
            {
                return new CommandModel(descriptor.Name, redirect);
            }
            else
            {
                return new CommandModel(descriptor.Name);
            }
        }

        private readonly IForestContext _context;
        private readonly ILogger _logger;

        internal ForestDomManager(IForestContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }
        
        private IEnumerable<DomNode> BuildDom(Tree tree)
        {
            var result = new LinkedList<DomNode>();
            var processedNodes = new Stack<Tree.Node>();
            var nodeMap = new Dictionary<string, DomNode>(StringComparer.Ordinal);
            foreach (var node in tree)
            {
                var viewDescriptor = _context.ViewRegistry.Describe(node.ViewHandle);
                var viewState = node.ViewState.Value;
                var commands = viewDescriptor.Commands.Values
                    .Where(cmd => !viewState.DisabledCommands.Contains(cmd.Name))
                    .Where(_context.SecurityManager.HasAccess)
                    .Select(x => CreateModel(x, viewState))
                    .ToDictionary(x => x.Name, x => x, StringComparer.Ordinal);
                var domNode = new DomNode(
                    node.Key, 
                    viewDescriptor.Name,
                    node.Region,
                    viewState.Model,
                    nodeMap.TryGetValue(node.ParentKey, out var parent) ? parent : null, 
                    ImmutableDictionary<string, IReadOnlyCollection<DomNode>>.Empty, 
                    ImmutableDictionary.CreateRange(commands.Comparer, commands),
                    viewState.ResourceBundle,
                    node.Revision
                );
                nodeMap[node.Key] = domNode;
                processedNodes.Push(node);
            }

            while (processedNodes.Count > 0)
            {
                var node = processedNodes.Pop();
                if (!nodeMap.TryGetValue(node.Key, out var domNode))
                {
                    continue;
                }
                if (nodeMap.TryGetValue(node.ParentKey, out var parentDomNode))
                {
                    var region = domNode.Region;
                    var newRegionContents = parentDomNode.Regions.TryGetValue(region, out var nodes)
                        ? new[] { domNode }.Union(nodes).ToArray()
                        : new[] { domNode };
                    var newRegions = parentDomNode.Regions
                        .Remove(region)
                        .Add(region, newRegionContents);
                    var newParent = new DomNode(
                        parentDomNode.InstanceID, 
                        parentDomNode.Name, 
                        parentDomNode.Region, 
                        parentDomNode.Model, 
                        parentDomNode.Parent, 
                        newRegions, 
                        parentDomNode.Commands,
                        parentDomNode.ResourceBundle,
                        parentDomNode.Revision);
                    var newNode = new DomNode(
                        domNode.InstanceID, 
                        domNode.Name, 
                        domNode.Region, 
                        domNode.Model, 
                        newParent, 
                        domNode.Regions, 
                        domNode.Commands,
                        domNode.ResourceBundle,
                        domNode.Revision);
                    nodeMap[parentDomNode.InstanceID] = newParent;
                    nodeMap[domNode.InstanceID] = newNode;
                }
                result.AddFirst(domNode);
            }

            return result;
        }


        public void ProcessDomNodes(Tree tree, Predicate<DomNode> isChanged, IEnumerable<IDomProcessor> domProcessors)
        {
            var processors = domProcessors.ToArray();
            var changedNodes = new Dictionary<string, DomNode>(StringComparer.Ordinal);
            foreach (var domNode in BuildDom(tree))
            {
                var dn = domNode;
                var isNodeChanged = isChanged(dn);
                foreach (var domProcessor in processors)
                {
                    if (dn.Parent != null && changedNodes.TryGetValue(dn.Parent.InstanceID, out var updatedParent))
                    {
                        dn = new DomNode(dn.InstanceID, dn.Name, dn.Region, dn.Model, updatedParent, dn.Regions, dn.Commands, dn.ResourceBundle, dn.Revision);
                    }

                    if (isNodeChanged)
                    {
                        _logger.Debug("Updating changed dom node {0}", dn.Name);
                    }
                    dn = domProcessor.ProcessNode(dn, isNodeChanged);
                    changedNodes[dn.InstanceID] = dn;
                }
            }
        }
    }
}