using Forest.ComponentModel;
using Forest.StateManagement;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Forest.UI
{
    using A1 = Func<DomNode, DomNode>;
    using A2 = Action<IEnumerable<DomNode>>;

    internal sealed class ForestDomRenderer : IForestStateVisitor
    {
        private readonly Func<DomNode, DomNode> _visit;
        private readonly Action<IEnumerable<DomNode>> _complete;
        private readonly IForestContext _context;

        /// Stores the rendered node state
        private ImmutableDictionary<string, DomNode> _nodeMap = ImmutableDictionary.Create<string, DomNode>(StringComparer.Ordinal);
        /// Stores the original view-model state. Used to determine which nodes to be re-rendered
        private ImmutableDictionary<string, object> _modelMap = ImmutableDictionary.Create<string, object>(StringComparer.Ordinal);
        /// A list holding the tuples of a view's hash and a boolean telling whether the view should be refreshed
        private ImmutableList<Tuple<string, bool>> _changeStateList = ImmutableList.Create<Tuple<string, bool>>();

        internal ForestDomRenderer(A1 visit, A2 complete, IForestContext cxt)
        {
            _visit = visit;
            _complete = complete;
            _context = cxt;
        }
        internal ForestDomRenderer(IEnumerable<IDomProcessor> renderChain, IForestContext context)
            : this(
                  x =>
                  {
                      var result = x;
                      foreach(var dp in renderChain)
                      {
                          result = dp.ProcessNode(result);
                      }
                      return result;
                  },
                  x =>
                  {
                      foreach (var dp in renderChain)
                      {
                          dp.Complete(x);
                      }
                  },
                  context) { }

        private ICommandModel CreateModel(ICommandDescriptor descriptor) => new CommandModel(descriptor.Name);

        private ILinkModel CreateModel(ILinkDescriptor descriptor) => new LinkModel(descriptor.Name);

        void IForestStateVisitor.BFS(Tree.Node treeNode, int index, ViewState viewState, IViewDescriptor descriptor)
        {
            // go ahead top-to-bottom and collect the basic model data
            var hash = treeNode.InstanceID;
            if (_context.SecurityManager.HasAccess(descriptor))
            {
                var commands = descriptor.Commands.Values
                    .Where(cmd => !viewState.DisabledCommands.Contains(cmd.Name))
                    .Where(_context.SecurityManager.HasAccess)
                    .Select(CreateModel)
                    .ToDictionary(x => x.Name, x => x, StringComparer.Ordinal);
                var links = descriptor.Links.Values
                    .Where(lnk => !viewState.DisabledLinks.Contains(lnk.Name))
                    .Where(_context.SecurityManager.HasAccess)
                    .Select(CreateModel)
                    .ToDictionary(x => x.Name, x => x, StringComparer.Ordinal);

                // TODO replace with revision comparison
                var canSkipRenderCall = _modelMap.TryGetValue(hash, out var model) && Equals(model, viewState.Model);
                if (!canSkipRenderCall)
                {
                    _modelMap = _modelMap.Add(hash, viewState.Model);
                }
                var node = new DomNode(hash, index, descriptor.Name, treeNode.Region, viewState.Model, null, ImmutableDictionary<string, IEnumerable<DomNode>>.Empty, ImmutableDictionary.CreateRange(commands.Comparer, commands), ImmutableDictionary.CreateRange(links.Comparer, links));
                _nodeMap = _nodeMap.Add(hash, node);
                _changeStateList = _changeStateList.Add(Tuple.Create(hash, canSkipRenderCall));
            }
        }
        void IForestStateVisitor.DFS(Tree.Node treeNode, int index, ViewState viewState, IViewDescriptor descriptor)
        {
            // go backwards bottom-to-top and properly update the hierarchy
            var parentKey = treeNode.Parent.InstanceID;
            if (_nodeMap.TryGetValue(parentKey, out var parent) && _nodeMap.TryGetValue(treeNode.InstanceID, out var node))
            {
                var region = treeNode.Region;
                var newRegionContents = parent.Regions.TryGetValue(region, out var nodes)
                    ? new[] { node }.Union(nodes)
                    : new[] { node };
                var newRegions = parent.Regions.Remove(region).Add(region, newRegionContents);
                var newParent = new DomNode(parent.InstanceID, parent.Index, parent.Name, parent.Region, parent.Model, parent.Parent, newRegions, parent.Commands, parent.Links);
                var newNode = new DomNode(node.InstanceID, node.Index, node.Name, node.Region, node.Model, newParent, node.Regions, node.Commands, node.Links);
                _nodeMap = _nodeMap
                    .Remove(parentKey).Add(parentKey, newParent)
                    .Remove(node.InstanceID).Add(node.InstanceID, newNode);
            }
        }
        void IForestStateVisitor.Complete()
        {
            var nodes = ImmutableList.Create<DomNode>();
            foreach(var item in _changeStateList)
            {
                var h = item.Item1;
                var skip = item.Item2;
                nodes = nodes.Add(skip ? _nodeMap[h] : _visit(_nodeMap[h]));
            }
            _complete(nodes);
            //// clean-up the accumulated state.
            _nodeMap = _nodeMap.Clear();
            _modelMap = _modelMap.Clear();
            _changeStateList = _changeStateList.Clear();
        }
    }
}
