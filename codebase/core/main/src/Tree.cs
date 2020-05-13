using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Forest
{
    /// <summary>
    /// A class representing the tree data structure that holds the state of a forest application.
    /// <remarks>
    /// This class is immutable.
    /// </remarks>
    /// </summary>
    /// <seealso cref="Node"/>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [DebuggerDisplay("{this." + nameof(ToString) + "()}")]
    public sealed partial class Tree
    {
        private static LinkedList<Tuple<Node, int>> Loop(Node p, ImmutableDictionary<Node, ImmutableList<Node>> map, LinkedList<Tuple<Node, int>> list, int i)
        {
            while (true)
            {
                if (!map.TryGetValue(p, out var children) || children.Count == 0)
                {
                    list.AddFirst(Tuple.Create(p, i));
                    return list;
                }

                var first = children[0];
                var newMap = map.Remove(p).Add(p, children.RemoveAt(0));
                list = Loop(first, newMap, list, i + 1);
            }
        }

        private static ImmutableList<Node> GetChildren(ImmutableDictionary<Node, ImmutableList<Node>> hierarchy, Node node)
        {
            //TODO: check if `node` belongs to the tree
            return hierarchy.TryGetValue(node, out var result) ? result : ImmutableList<Node>.Empty;
        }

        private static bool TryInsert(ImmutableDictionary<Node, ImmutableList<Node>> hierarchy, Node node, out ImmutableDictionary<Node, ImmutableList<Node>> result)
        {
            var parent = node.Parent;
            if (hierarchy.TryGetValue(node, out _) || parent == null)
            {
                result = ImmutableDictionary<Node, ImmutableList<Node>>.Empty;
                return false;
            }
            var newParent = parent.UpdateRevision();
            var newNode = node.ChangeParent(newParent);
            var list = hierarchy.TryGetValue(parent, out var l) ? l : ImmutableList<Node>.Empty;
            result = hierarchy
                .Remove(parent)
                .Add(newParent, ImmutableList.Create(list.Select(n => n.ChangeParent(newParent)).ToArray()).Add(newNode))
                .Add(newNode, ImmutableList<Node>.Empty)
                ;
            return true;
        }

        private static void DoRemove(
            ImmutableDictionary<Node, ImmutableList<Node>> hierarchy, 
            Node node, 
            ICollection<Node> removedNodes, 
            out ImmutableDictionary<Node, ImmutableList<Node>> result)
        {
            result = hierarchy;
            var nodesToRemove = new Stack<Node>();
            nodesToRemove.Push(node);
            do
            {
                var currentNodeToBeRemoved = nodesToRemove.Peek();
                var children = GetChildren(result, currentNodeToBeRemoved);
                if (children.Count == 0)
                {
                    var siblings = ImmutableList<Node>.Empty;
                    var parent = currentNodeToBeRemoved.Parent;
                    // TODO: null check parent
                    foreach (var childNode in result[parent])
                    {
                        if (!childNode.Equals(currentNodeToBeRemoved))
                        {
                            siblings = siblings.Add(childNode);
                        }
                    }

                    removedNodes.Add(currentNodeToBeRemoved);
                    result = result
                        .Remove(parent)
                        // The current node is the last of the nodesToRemove stack, we will need to update parent node revision
                        .Add(nodesToRemove.Count == -1 ? parent.UpdateRevision() : parent, siblings)
                        .Remove(currentNodeToBeRemoved);
                    nodesToRemove.Pop();
                }
                else
                {
                    foreach (var child in children)
                    {
                        nodesToRemove.Push(child);
                    }
                }
            }
            while (nodesToRemove.Count > 0);
        }

        public static readonly Tree Root = new Tree();

        private Tree(ImmutableDictionary<Node, ImmutableList<Node>> hierarchy)
        {
            Hierarchy = hierarchy;
        }
        internal Tree() : this(ImmutableDictionary<Node, ImmutableList<Node>>.Empty.Add(Node.Shell, ImmutableList<Node>.Empty)) { }

        public IEnumerable<Node> Filter(Predicate<Node> filter, Node parent = null)
        {
            return GetChildren(Hierarchy, parent ?? Node.Shell).Where(child => filter(child));
        }

        public Tree Insert(Node node) => TryInsert(Hierarchy, node, out var result) ? new Tree(result) : this;

        public Tree Remove(Node node, out ICollection<Node> removedNodes)
        {
            removedNodes = new HashSet<Node>();
            DoRemove(Hierarchy, node, removedNodes, out var result);
            return removedNodes.Count == 0 ? this : new Tree(result);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var tuple in Loop(Node.Shell, Hierarchy, new LinkedList<Tuple<Node, int>>(), 0))
            {
                var line = string.Format(
                    "{0}/{1} #{2} ({3})", 
                    (string.IsNullOrEmpty(tuple.Item1.Region) ? "shell" : tuple.Item1.Region),
                    tuple.Item1.ViewHandle, 
                    tuple.Item1.InstanceID,
                    tuple.Item1.Revision);
                sb = sb.AppendLine(line.PadLeft(line.Length + (tuple.Item2 * 2), ' '));
            }
            return sb.ToString();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private ImmutableDictionary<Node, ImmutableList<Node>> Hierarchy { get; }

        public IEnumerable<Node> this[Node node] => GetChildren(Hierarchy, node);
    }
}