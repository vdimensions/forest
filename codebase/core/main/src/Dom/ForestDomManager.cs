﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Forest.ComponentModel;

namespace Forest.Dom
{
    internal sealed class ForestDomManager : IForestDomManager
    {
        private static ICommandModel CreateModel(ICommandDescriptor descriptor) => new CommandModel(descriptor.Name);

        private readonly IForestContext _context;

        internal ForestDomManager(IForestContext context)
        {
            _context = context;
        }
        
        private IEnumerable<DomNode> BuildDom(Tree tree)
        {
            var result = new LinkedList<DomNode>();
            var processedNodes = new Stack<Tree.Node>();
            var nodeMap = new Dictionary<string, DomNode>(StringComparer.Ordinal);
            foreach (var node in tree)
            {
                var viewDescriptor = _context.ViewRegistry.GetDescriptor(node.ViewHandle);
                var viewState = node.ViewState.Value;
                var commands = viewDescriptor.Commands.Values
                    .Where(cmd => !viewState.DisabledCommands.Contains(cmd.Name))
                    .Where(_context.SecurityManager.HasAccess)
                    .Select(CreateModel)
                    .ToDictionary(x => x.Name, x => x, StringComparer.Ordinal);
                var domNode = new DomNode(
                    node.Key, 
                    viewDescriptor.Name,
                    node.Region,
                    viewState.Model,
                    nodeMap.TryGetValue(node.ParentKey, out var parent) ? parent : null, 
                    ImmutableDictionary<string, IEnumerable<DomNode>>.Empty, 
                    ImmutableDictionary.CreateRange(commands.Comparer, commands)
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
                        ? new[] { domNode }.Union(nodes)
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
                        parentDomNode.Commands);
                    var newNode = new DomNode(
                        domNode.InstanceID, 
                        domNode.Name, 
                        domNode.Region, 
                        domNode.Model, 
                        newParent, 
                        domNode.Regions, 
                        domNode.Commands);
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
            foreach (var domNode in BuildDom(tree))
            {
                var dn = domNode;
                var isNodeChanged = isChanged(dn);
                foreach (var domProcessor in processors)
                {
                    dn = domProcessor.ProcessNode(dn, isNodeChanged);
                }
            }
        }
    }
}