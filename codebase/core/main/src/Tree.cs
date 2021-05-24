using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Axle.Collections.Immutable;
using Forest.ComponentModel;

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
    public sealed class Tree : IEnumerable<Tree.Node>
    {
        #region Node
        /// <summary>
        /// A struct representing a single node inside a Forest <see cref="Tree"/>.
        /// </summary>
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [Serializable]
        #endif
        [StructLayout(LayoutKind.Sequential)]
        [DebuggerDisplay("{this." + nameof(ToString) + "()}")]
        public struct Node : IComparable<Node>, IEquatable<Node>
        {
            internal static Node Create(string key, ViewHandle viewHandle, string region, object model, Node parent)
            {
                var viewState = model == null ? null : new ViewState?(Forest.ViewState.Create(model, null));
                
                return new Node(parent.Key, region, viewHandle, key, viewState, 0u);
            }

            public static readonly Node Shell;
            
            private readonly string _region;
            private readonly ViewHandle _viewHandle;
            private readonly string _key;
            private readonly string _parentKey;

            internal Node(Node parent, string region, ViewHandle viewHandle, string key) 
                : this(parent.Key, region, viewHandle, key, null, 0u) { }
            private Node(string parentKey, string region, ViewHandle viewHandle, string key, ViewState? viewState, uint revision)
            {
                _key = key;
                _parentKey = parentKey;
                _region = region;
                _viewHandle = viewHandle;
                ViewState = viewState;
                Revision = revision;
            }
            
            int IComparable<Node>.CompareTo(Node other)
            {
                return StringComparer.Ordinal.Compare(Key, other.Key);
            }

            /// <inheritdoc />
            public bool Equals(Node other)
            {
                return StringComparer.Ordinal.Equals(Key, other.Key);
            }

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Node other && Equals(other);

            /// <inheritdoc />
            public override int GetHashCode() => (Key != null ? Key.GetHashCode() : 0);

            internal Node UpdateRevision() => new Node(ParentKey, Region, ViewHandle, Key, ViewState, 1u + Revision);

            internal Node SetViewState(ViewState viewState)
            {
                return new Node(ParentKey, Region, ViewHandle, Key, viewState, Revision);
            }

            public string ParentKey => _parentKey ?? string.Empty;

            /// <summary>
            /// Gets the name of the <see cref="IRegion"/> that contains the view represented by this <see cref="Node"/>.
            /// </summary>
            public string Region => _region ?? string.Empty;

            /// <summary>
            /// Gets a <see cref="ViewHandle"/> that can be used to obtain the 
            /// <see cref="IForestViewDescriptor">view descriptor</see>
            /// </summary>
            public ViewHandle ViewHandle => _viewHandle ?? ViewHandle.FromName(string.Empty);

            /// <summary>
            /// A unique string identifying the current node.
            /// </summary>
            public string Key => _key ?? Guid.Empty.ToString();
            
            public ViewState? ViewState { get; }
            public uint Revision { get; }

            //public string RegionSegment => ToStringBuilder(true).ToString();
        }
        #endregion
        
        private static ImmutableList<Node> GetChildren(
            ImmutableDictionary<string, Node> nodes, 
            ImmutableDictionary<string, ImmutableList<string>> hierarchy, 
            string node)
        {
            return hierarchy.TryGetValue(node ?? string.Empty, out var result) 
                ? ImmutableList.CreateRange(result.Select(k => nodes[k]).ToArray()) 
                : ImmutableList<Node>.Empty;
        }

        private static bool TryInsert(TreeChangeScope scope, Tree tree, Node node, out Tree newTree)
        {
            var newNodes = tree._nodes;
            var newHierarchy = tree._hierarchy;
            if (newNodes.TryGetValue(node.Key, out _) || !newNodes.TryGetValue(node.ParentKey, out var parent))
            {
                newTree = tree;
                return false;
            }
            var parentKey = parent.Key;
            newNodes = newNodes
                .Remove(parentKey)
                .Add(parentKey, scope.UpdateRevision(parent))
                .Add(node.Key, scope.UpdateRevision(node));
            var siblingsKeys = newHierarchy.TryGetValue(node.ParentKey, out var l) ? l : ImmutableList<string>.Empty;
            newHierarchy = newHierarchy
                .Remove(parentKey)
                .Add(parentKey, siblingsKeys.Add(node.Key))
                .Add(node.Key, ImmutableList<string>.Empty);
            newTree = new Tree(newNodes, newHierarchy);
            return true;
        }

        /// <summary>
        /// Gets a reference to an empty <see cref="Tree"/> instance.
        /// </summary>
        public static readonly Tree Root = new Tree();
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ImmutableDictionary<string, Node> _nodes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ImmutableDictionary<string, ImmutableList<string>> _hierarchy;

        private Tree(
            ImmutableDictionary<string, Node> nodes, 
            ImmutableDictionary<string, ImmutableList<string>> hierarchy)
        {
            _nodes = nodes;
            _hierarchy = hierarchy;
        }
        private Tree(IEqualityComparer<string> keyComparer) : this(
            ImmutableDictionary.Create<string, Node>(keyComparer).Add(Node.Shell.Key, Node.Shell),
            ImmutableDictionary.Create<string, ImmutableList<string>>(keyComparer).Add(Node.Shell.Key, ImmutableList<string>.Empty)) { }
        internal Tree() : this(StringComparer.Ordinal) { }

        public IEnumerable<Node> Filter(Predicate<Node> filter, string parent = null)
        {
            return GetChildren(_nodes, _hierarchy, parent).Where(child => filter(child));
        }

        public Tree Insert(
            TreeChangeScope scope, 
            string key, 
            ViewHandle viewHandle, 
            string region, 
            string ownerKey, 
            object model, 
            out Node node)
        {
            if (!TryFind(ownerKey, out var parent))
            {
                //TODO: throw new NodeNotFoundException(node);
                throw new InvalidOperationException("Cannot find node!");
            }
            var tree = TryInsert(
                scope, 
                this, 
                Node.Create(key, viewHandle, region, model, parent), out var result) ? result : this;
            node = tree[key].Value;
            return tree;
        }

        public Tree Remove(TreeChangeScope scope, string key, out ICollection<Node> removedNodes)
        {
            removedNodes = new HashSet<Node>();
            var newNodes = _nodes;
            var newHierarchy = _hierarchy;
            var keysToRemove = new Stack<string>();
            keysToRemove.Push(key);
            do
            {
                var currentKeyToBeRemoved = keysToRemove.Peek();
                var currentNodeToBeRemoved = newNodes[currentKeyToBeRemoved];
                var children = newHierarchy.TryGetValue(currentKeyToBeRemoved, out var c)
                    ? c
                    : ImmutableList<string>.Empty;
                if (children.Count == 0)
                {
                    var siblingKeys = ImmutableList<string>.Empty;
                    var parentKey = currentNodeToBeRemoved.ParentKey;
                    // TODO: null check parent
                    foreach (var siblingKey in newHierarchy[parentKey])
                    {
                        if (!newHierarchy.KeyComparer.Equals(siblingKey, currentKeyToBeRemoved))
                        {
                            siblingKeys = siblingKeys.Add(siblingKey);
                        }
                    }

                    removedNodes.Add(currentNodeToBeRemoved);
                    newHierarchy = newHierarchy
                        .Remove(parentKey)
                        .Add(parentKey, siblingKeys)
                        .Remove(currentKeyToBeRemoved);
                    newNodes = newNodes.Remove(currentKeyToBeRemoved);
                    if (keysToRemove.Count == 1)
                    {
                        var newParent = scope.UpdateRevision(newNodes[parentKey]);
                        newNodes = newNodes.Remove(parentKey).Add(parentKey, newParent);
                    }
                    keysToRemove.Pop();
                }
                else
                {
                    foreach (var child in children)
                    {
                        keysToRemove.Push(child);
                    }
                }
            }
            while (keysToRemove.Count > 0);
            
            return removedNodes.Count == 0 ? this : new Tree(newNodes, newHierarchy);
        }

        public bool TryFind(string key, out Node node) => _nodes.TryGetValue(key, out node);

        internal Tree SetViewState(TreeChangeScope scope, string key, ViewState viewState)
        {
            if (!_nodes.TryGetValue(key, out var targetNode))
            {
                //TODO: throw new NodeNotFoundException(node);
                throw new InvalidOperationException("Cannot find node!");
            }
            return new Tree(
                _nodes.Remove(key).Add(key, scope.UpdateRevision(targetNode.SetViewState(viewState))),
                _hierarchy);
        }

        internal Tree UpdateViewState(TreeChangeScope scope, string key, Func<ViewState, ViewState> viewStateUpdateFn)
        {
            if (!_nodes.TryGetValue(key, out var targetNode))
            {
                //TODO: throw new NodeNotFoundException(node);
                throw new InvalidOperationException("Cannot find node!");
            }
            return new Tree(
                _nodes.Remove(key).Add(key, scope.UpdateRevision(targetNode.SetViewState(viewStateUpdateFn(targetNode.ViewState ?? ViewState.Empty)))),
                _hierarchy);
        }

        public IEnumerable<Node> GetChildren(string key)
        {
            return _hierarchy.TryGetValue(key, out var childrenKeys)
                ? childrenKeys
                    .Select(ck => _nodes.TryGetValue(ck, out var node) ? new Node?(node) : null)
                    .Where(n => n.HasValue)
                    .Select(n => n.Value)
                : Enumerable.Empty<Node>();
        }
        
        [SuppressMessage("ReSharper", "CognitiveComplexity")]
        private IEnumerable<Tuple<Node, int>> Traverse(string key, int initialDepth = -1)
        {
            if (_hierarchy.TryGetValue(key, out var childrenKeys))
            {
                foreach (var childKey in childrenKeys)
                {
                    if (_nodes.TryGetValue(childKey, out var childNode))
                    {
                        var newDepth = 1 + initialDepth;
                        yield return Tuple.Create(childNode, newDepth);
                        foreach (var child in Traverse(childNode.Key, newDepth))
                        {
                            yield return child;
                        }
                    }
                }
            }
        }
        
        public IEnumerator<Node> GetEnumerator() => Traverse(Node.Shell.Key).Select(x => x.Item1).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var tuple in Traverse(Node.Shell.Key))
            {
                var line = string.Format(
                    "{0}/{1} #{2}", 
                    (string.IsNullOrEmpty(tuple.Item1.Region) ? "shell" : tuple.Item1.Region),
                    tuple.Item1.ViewHandle, 
                    tuple.Item1.Key);
                sb = sb.AppendLine(line.PadLeft(line.Length + (tuple.Item2 * 2), ' '));
            }
            return sb.ToString();
        }

        public Node? this[string key] => TryFind(key, out var result) ? new Node?(result) : null;
    }
}