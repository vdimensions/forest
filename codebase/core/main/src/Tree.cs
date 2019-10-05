using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Forest
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [DebuggerDisplay("{this." + nameof(ToString) + "()}")]
    public class Tree
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [Serializable]
        #endif
        [DebuggerDisplay("{this." + nameof(ToString) + "()}")]
        public sealed class Node : IComparable<Node>, IEquatable<Node>
        {
            public static Node Create(string region, ViewHandle viewHandle, Node parent)
            {
                return new Node(parent, region, viewHandle, GuidGenerator.NewID().ToString());
            }
            public static Node Create(string region, string viewName, Node parent)
            {
                return new Node(parent, region, ViewHandle.FromName(viewName), GuidGenerator.NewID().ToString());
            }
            public static Node Create(string region, Type viewType, Node parent)
            {
                return new Node(parent, region, ViewHandle.FromType(viewType), GuidGenerator.NewID().ToString());
            }

            public static readonly Node Shell = new Node(null, string.Empty, ViewHandle.FromName(string.Empty), Guid.Empty.ToString());

            internal Node(Node parent, string region, ViewHandle viewHandle, string instanceID)
            {
                Parent = parent;
                Region = region;
                ViewHandle = viewHandle;
                InstanceID = instanceID;
            }

            public int CompareTo(Node other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                return string.Compare(InstanceID, other.InstanceID, StringComparison.Ordinal);
            }

            public bool Equals(Node other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return InstanceID == other.InstanceID;
            }

            public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Node other && Equals(other);

            public override int GetHashCode() => (InstanceID != null ? InstanceID.GetHashCode() : 0);

            private StringBuilder ToStringBuilder(bool stopAtRegion)
            {
                var sb = Parent == null
                    ? new StringBuilder() 
                    : Parent.ToStringBuilder(false).Append(Region.Length == 0 ? "shell" : Region);
                return (stopAtRegion ? sb : sb.AppendFormat("/{0} #{1}", ViewHandle, InstanceID)).Append('/');
            }

            public override string ToString() => ToStringBuilder(false).ToString();

            public Node Parent { get; }
            public string Region { get; }
            public ViewHandle ViewHandle { get; }
            public string InstanceID { get; }
            public string RegionSegment => ToStringBuilder(true).ToString();
        }

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

        private static ImmutableList<Node> GetChildren(ImmutableDictionary<Node, ImmutableList<Node>> hierarchy, Node node) => 
            hierarchy.TryGetValue(node, out var result) ? result : ImmutableList<Node>.Empty;
        
        public static bool TryInsert(ImmutableDictionary<Node, ImmutableList<Node>> hierarchy, Node node, out ImmutableDictionary<Node, ImmutableList<Node>> result)
        {
            if (hierarchy.TryGetValue(node, out _))
            {
                result = ImmutableDictionary<Node, ImmutableList<Node>>.Empty;
                return false;
            }
            var parent = node.Parent; // TOOD: null check
            var list = hierarchy.TryGetValue(parent, out var l) ? l : ImmutableList<Node>.Empty;
            result = hierarchy
                .Remove(parent)
                .Add(parent, list.Add(node))
                .Add(node, ImmutableList<Node>.Empty);
            return true;
        }

        private static void DoRemove(ImmutableDictionary<Node, ImmutableList<Node>> hierarchy, Node node, ICollection<Node> removedNodes, out ImmutableDictionary<Node, ImmutableList<Node>> result)
        {
            result = hierarchy;
            var items = new Stack<Node>();
            items.Push(node);
            do
            {
                var n = items.Peek();
                var children = GetChildren(result, n);
                if (children.Count == 0)
                {
                    var ns = ImmutableList<Node>.Empty;
                    // TODO: null check parent
                    foreach (var childNode in result[n.Parent])
                    {
                        if (!childNode.Equals(n))
                        {
                            ns = ns.Add(childNode);
                        }
                    }

                    removedNodes.Add(n);
                    result = result.Remove(n.Parent).Add(n.Parent, ns).Remove(n);
                    items.Pop();
                }
                else
                {
                    foreach (var child in children)
                    {
                        items.Push(child);
                    }
                }
            }
            while (items.Count > 0);
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
                    "{0}/{1} #{2}", 
                    (string.IsNullOrEmpty(tuple.Item1.Region) ? "shell" : tuple.Item1.Region),
                    tuple.Item1.ViewHandle, 
                    tuple.Item1.InstanceID);
                sb = sb.AppendLine(line.PadLeft(line.Length + (tuple.Item2 * 2), ' '));
            }
            return sb.ToString();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private ImmutableDictionary<Node, ImmutableList<Node>> Hierarchy { get; }

        public IEnumerable<Node> Roots => Hierarchy.Keys;

        public IEnumerable<Node> this[Node node] => GetChildren(Hierarchy, node);//TODO: check belongs
    }
}