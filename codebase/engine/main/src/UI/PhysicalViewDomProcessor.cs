using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Forest.Engine;

namespace Forest.UI
{
    internal sealed class PhysicalViewDomProcessor : IDomProcessor
    {
        public enum NodeState
        {
            NewNode = 0,
            UpdatedNode = 1
        }
        private IImmutableDictionary<string, IPhysicalView> _physicalViews;
        private IImmutableSet<string> _nodesToPreserve = ImmutableHashSet<string>.Empty;
        private IImmutableList<Tuple<DomNode, NodeState>> _nodeStates = ImmutableList<Tuple<DomNode, NodeState>>.Empty;

        private readonly IForestEngine _engine;
        private readonly IPhysicalViewRenderer _renderer;

        internal PhysicalViewDomProcessor(IDictionary<string, IPhysicalView> physicalViews, IForestEngine engine, IPhysicalViewRenderer renderer)
        {
            _physicalViews = physicalViews == null 
                ? ImmutableDictionary.Create<string, IPhysicalView>(StringComparer.Ordinal) 
                : ImmutableDictionary.CreateRange(StringComparer.Ordinal, physicalViews);
            _engine = engine;
            _renderer = renderer;
        }
        public PhysicalViewDomProcessor(IForestEngine engine, IPhysicalViewRenderer renderer) : this(null, engine, renderer) { }

        [Obsolete]
        public IImmutableDictionary<string, IPhysicalView> PhysicalViews
        {
            get => _physicalViews;
            set => _physicalViews = value;
        }

        DomNode IDomProcessor.ProcessNode(DomNode node)
        {
            _nodesToPreserve = _nodesToPreserve.Add(node.InstanceID);
            _nodeStates = _nodeStates.Add(
                Tuple.Create(
                    node, 
                    _physicalViews.TryGetValue(node.InstanceID, out _) ? NodeState.UpdatedNode : NodeState.NewNode));
            return node;
        }

        void IDomProcessor.Complete(IEnumerable<DomNode> nodes)
        {
            var physicalViews = new Dictionary<string, IPhysicalView>(StringComparer.Ordinal);
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
            foreach(var nodeState in _nodeStates.Reverse())
            {
                var n = nodeState.Item1;
                //var p = n.Parent;
                var isNewView = nodeState.Item2 == NodeState.NewNode;
                //var parent = p != null && physicalViews.TryGetValue(p.InstanceID, out var _p) ? _p : null;
                var current = physicalViews.TryGetValue(n.InstanceID, out var _n) ? _n : null;

                IPhysicalView parent = null;
                if (n.Parent == null || (n.Parent != null && physicalViews.TryGetValue(n.Parent.InstanceID, out parent)))
                {
                    if (current == null && isNewView)
                    {
                        var physicalView = parent != null
                            ? _renderer.CreateNestedPhysicalView(_engine, parent, n)
                            : _renderer.CreatePhysicalView(_engine, n);
                        //updateCallArguments < -(physicalView, n)::updateCallArguments
                        updateCallArguments.Add(Tuple.Create(physicalView, n));
                        physicalViews.Add(n.InstanceID, physicalView);
                    }
                    else if (current != null && !isNewView)
                    {
                        updateCallArguments.Add(Tuple.Create(current, n));
                    }
                    else if (current != null && isNewView)
                    {
                        if (n.Parent != null)
                        {
                            throw new InvalidOperationException(string.Format("Did not expect physical view {0} #{1} to contain child {2} #{3}", n.Parent.Name, n.Parent.InstanceID, n.Name, n.InstanceID));
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("Physical view {0} #{1} already exists", n.Name, n.InstanceID));
                        }
                    }
                    else if (current == null && !isNewView)
                    {
                        throw new InvalidOperationException(string.Format("Could not locate physical view {0} #{1}", n.Name, n.InstanceID));
                    }
                }
                else if (n.Parent != null && parent == null)
                {
                    //var x = n.Parent == null;
                    //var y = parent == null;
                    // TODO: exception: parent not found
                    throw new InvalidOperationException(string.Format("Could not locate physical view {0} #{1} that should be parent of {2} #{3}", n.Parent.Name, n.Parent.InstanceID, n.Name, n.InstanceID));
                }
            }

            _physicalViews = ImmutableDictionary.CreateRange(StringComparer.Ordinal, physicalViews);
            _nodesToPreserve = ImmutableHashSet.Create<string>(StringComparer.Ordinal);
            _nodeStates = ImmutableList.Create<Tuple<DomNode, NodeState>>();

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
                x.Item1.Update(x.Item2);
            }
        }
    }
}
