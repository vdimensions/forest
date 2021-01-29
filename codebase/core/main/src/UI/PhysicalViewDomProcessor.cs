using System;
using System.Collections.Generic;
using Axle.Collections.Immutable;
using Forest.Dom;
using Forest.Engine;

namespace Forest.UI
{
    internal sealed class PhysicalViewDomProcessor : IDomProcessor
    {
        public enum NodeState : sbyte
        {
            Unchanged = 0,
            NewNode = 1,
            UpdatedNode = 2,
        }

        private IImmutableHashSet<string> _nodesToPreserve = ImmutableHashSet<string>.Empty;
        private IImmutableList<Tuple<DomNode, NodeState>> _nodeStates = ImmutableList<Tuple<DomNode, NodeState>>.Empty;

        private readonly IForestEngine _engine;
        private readonly IPhysicalViewRenderer _renderer;
        private readonly ImmutableDictionary<string, IPhysicalView> _physicalViews;

        internal PhysicalViewDomProcessor(
            IForestEngine engine, 
            IPhysicalViewRenderer renderer, 
            ImmutableDictionary<string, IPhysicalView> physicalViews)
        {
            _engine = engine;
            _renderer = renderer;
            _physicalViews = physicalViews == null
                ? ImmutableDictionary.Create<string, IPhysicalView>(StringComparer.Ordinal)
                : ImmutableDictionary.CreateRange(physicalViews.KeyComparer, physicalViews);
        }

        DomNode IDomProcessor.ProcessNode(DomNode node, bool isNodeUpdated)
        {
            _nodesToPreserve = _nodesToPreserve.Add(node.InstanceID);
            var physicalViewExists = _physicalViews.TryGetValue(node.InstanceID, out var pv);
            //
            // Because of globalization or other processor interference, the dom node passed-in may be different
            // than the dom node that is in possession of the physical view.
            // In case there were no changes to that particular node, we must assume that
            // the physical view is the source of truth. 
            //
            var actualNode = physicalViewExists 
                ? isNodeUpdated ? (pv.Node ?? node) : node 
                : node;
            _nodeStates = _nodeStates.Add(
                Tuple.Create(
                    actualNode, 
                    physicalViewExists
                        ? isNodeUpdated ? NodeState.UpdatedNode : NodeState.Unchanged 
                        : NodeState.NewNode));
            return node;
        }

        public ImmutableDictionary<string, IPhysicalView> RenderViews()
        {
            var physicalViews = new Dictionary<string, IPhysicalView>(_physicalViews.KeyComparer);
            foreach (var kvp in _physicalViews)
            {
                if (_nodesToPreserve.Contains(kvp.Key))
                {
                    physicalViews.Add(kvp.Key, kvp.Value);
                }
                else
                {
                    //
                    // The respective logical view was removed and is no longer present in the state.
                    //
                    kvp.Value.Dispose();
                }
            }

            // store tuples of renderer and node to initiate an update call after the views are rendered
            var updateCallArguments = new List<Tuple<IPhysicalView, DomNode>>();
            foreach(var nodeStateTuple in _nodeStates)
            {
                var n = nodeStateTuple.Item1;
                var nodeState = nodeStateTuple.Item2;
                var current = physicalViews.TryGetValue(n.InstanceID, out var _n) ? _n : null;

                IPhysicalView parent = null;
                if (n.Parent == null || (n.Parent != null && physicalViews.TryGetValue(n.Parent.InstanceID, out parent)))
                {
                    if (current == null && nodeState == NodeState.NewNode)
                    {
                        var physicalView = parent != null
                            ? _renderer.CreateNestedPhysicalView(_engine, parent, n)
                            : _renderer.CreatePhysicalView(_engine, n);
                        updateCallArguments.Add(Tuple.Create(physicalView, n));
                        physicalViews.Add(n.InstanceID, physicalView);
                    }
                    else if (current != null && nodeState != NodeState.Unchanged)
                    {
                        updateCallArguments.Add(Tuple.Create(current, n));
                    }
                    else if (current != null && nodeState == NodeState.NewNode)
                    {
                        throw new InvalidOperationException(n.Parent != null
                            ? string.Format(
                                "Did not expect physical view {0} #{1} to contain child {2} #{3}", 
                                n.Parent.Name, 
                                n.Parent.InstanceID, 
                                n.Name, 
                                n.InstanceID)
                            : string.Format(
                                "Physical view {0} #{1} already exists", 
                                n.Name, 
                                n.InstanceID));
                    }
                    else if (current == null && nodeState != NodeState.NewNode)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "Could not locate physical view {0} #{1}", 
                                n.Name, 
                                n.InstanceID));
                    }
                }
                else if (n.Parent != null && parent == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Could not locate physical view {0} #{1} that should be parent of {2} #{3}", 
                            n.Parent.Name, 
                            n.Parent.InstanceID, 
                            n.Name, 
                            n.InstanceID));
                }
            }

            _nodesToPreserve = _nodesToPreserve.Clear();
            _nodeStates = _nodeStates.Clear();

            // Initiate the update calls for the enumerated physical views.
            //
            // Note that this happens AFTER all physical views have been collected.
            // If we attempt to call Update on a physical view without being done with the above rendering,
            // we risk that the call might trigger a Forest command or message passing instruction,
            // which in turn would cause a nested render call within the current render, while it is still operating.
            //
            // The consequences of the above situation could result in rendering existing physical view twice,
            // which has unknown consequences.
            // In addition, only the results of the outermost render call will be respected,
            // meaning any views created in the nested call(s) will not be displayed, while still residing in the memory.
            // 
            // At this point, invoking those update calls is safe and the described above side-effects are avoided.
            foreach (var x in updateCallArguments)
            {
                var physicalView = x.Item1;
                var domNode = x.Item2;
                physicalView.Update(domNode);
            }
            
            return ImmutableDictionary.CreateRange(physicalViews.Comparer, physicalViews);
        }
    }
}
