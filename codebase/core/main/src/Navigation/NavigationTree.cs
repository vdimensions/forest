using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Axle.Verification;

namespace Forest.Navigation
{
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NavigationTree
    {
        private static ICollection<string> ExpandChildren(
            string node, 
            IImmutableDictionary<string, IImmutableList<string>> hierarchy, 
            ICollection<string> children)
        {
            if (hierarchy.TryGetValue(node, out var nc))
            {
                foreach (var child in nc)
                {
                    ExpandChildren(child, hierarchy, children).Add(child);
                }
            }
            return children;
        }
        private static ICollection<string> ExpandAncestors(
            string node, 
            IImmutableDictionary<string, string> inverseHierarchy, 
            ICollection<string> ancestors)
        {
            if (inverseHierarchy.TryGetValue(node, out var parent))
            {
                ExpandAncestors(parent, inverseHierarchy, ancestors);
            }
            ancestors.Add(node);
            return ancestors;
        }

        private static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;
        // TODO: make internal
        internal const string Root = "";

        public NavigationTree() 
            : this(
                ImmutableDictionary.Create<string, IImmutableList<string>>(Comparer),
                ImmutableDictionary.Create<string, string>(Comparer),
                ImmutableDictionary.Create<string, object>(Comparer), 
                ImmutableHashSet.Create(Comparer)) { }
        private NavigationTree(
            IImmutableDictionary<string, IImmutableList<string>> hierarchy, 
            IImmutableDictionary<string, string> inverseHierarchy, 
            IImmutableDictionary<string, object> values, 
            ImmutableHashSet<string> state)
        {
            Hierarchy = hierarchy;
            InverseHierarchy = inverseHierarchy;
            Values = values;
            State = state;
        }

        public NavigationTree RegisterNavigationNode(string parent, string current, object message = null)
        {
            parent.VerifyArgument(nameof(parent)).IsNotNull();
            current.VerifyArgument(nameof(current)).IsNotNull();
            
            var inverseHierarchy = InverseHierarchy;
            var hierarchy = Hierarchy;
            var messageData = Values;
            var selectedState = State;
            if (!Comparer.Equals(parent, Root) && !inverseHierarchy.ContainsKey(parent))
            {
                throw new ArgumentException(string.Format("Invalid navigation hierarchy: parent structure '{0}' was not found", parent));
            }
            if (!inverseHierarchy.TryGetValue(current, out var existingParent))
            {
                inverseHierarchy = inverseHierarchy.Add(current, existingParent = parent);
            }
            else if (!Comparer.Equals(parent, existingParent))
            {
                throw new ArgumentException(
                    string.Format("Existing navigation structure '{1}' already contains the provided node '{0}'", current, existingParent));
            }
            if (!hierarchy.TryGetValue(existingParent, out var children))
            {
                children = ImmutableList.Create<string>();
            }

            if (!children.Contains(current, Comparer))
            {
                children = children.Add(current);
            }
            hierarchy = hierarchy.Remove(existingParent).Add(existingParent, children);

            messageData = messageData.Remove(current);
            
            if (message != null)
            {
                messageData = messageData.Add(current, message);
            }
            
            return new NavigationTree(hierarchy, inverseHierarchy, messageData, selectedState);
        }
        
        public NavigationTree UnregisterNavigationNode(string node)
        {
            node.VerifyArgument(nameof(node)).IsNotNull();
            
            var inverseHierarchy = InverseHierarchy;
            var hierarchy = Hierarchy;
            var messageData = Values;
            var selectedState = State;
            
            if (inverseHierarchy.TryGetValue(node, out var parentNode) && hierarchy.TryGetValue(parentNode, out var siblings))
            {
                hierarchy = hierarchy.Remove(parentNode).Add(parentNode, siblings.Remove(node));
            }

            foreach (var childToRemove in ExpandChildren(node, Hierarchy, new HashSet<string>(Comparer)))
            {
                messageData = messageData.Remove(childToRemove);
                hierarchy = hierarchy.Remove(childToRemove);
                inverseHierarchy = inverseHierarchy.Remove(childToRemove);
            }
            
            return new NavigationTree(hierarchy, inverseHierarchy, messageData, selectedState);
        }

        public bool ToggleNode(string node, bool selected, out NavigationTree tree)
        {
            node.VerifyArgument(nameof(node)).IsNotNull();
            
            var hierarchy = Hierarchy;
            
            if (selected == State.Contains(node))
            {
                var children = ExpandChildren(node, hierarchy, new HashSet<string>(Comparer));
                if (children.All(x => !State.Contains(x)))
                {
                    tree = this;
                    return false;
                }
            }
            
            var inverseHierarchy = InverseHierarchy;
            var messageData = ImmutableDictionary<string, object>.Empty;
            var selectedState = State.Clear();

            if (selected)
            {
                var ancestors = ExpandAncestors(node, inverseHierarchy, new HashSet<string>(Comparer));
                foreach (var ancestor in ancestors)
                {
                    selectedState = selectedState.Add(ancestor);
                    if (Values.TryGetValue(ancestor, out var data))
                    {
                        messageData = messageData.Add(ancestor, data);
                    }
                }
            }
            
            tree = new NavigationTree(hierarchy, inverseHierarchy, messageData, selectedState.Remove(Root));
            return true;
        }
        
        public IEnumerable<string> GetChildren(string node)
        {
            node.VerifyArgument(nameof(node)).IsNotNull();
            
            return Hierarchy.TryGetValue(node, out var children)
                ? children as IEnumerable<string>
                : new string[0];
        }
        
        public IEnumerable<string> GetSiblings(string node)
        {
            node.VerifyArgument(nameof(node)).IsNotNull();
            
            return InverseHierarchy.TryGetValue(node, out var parent)
                ? GetChildren(parent)
                : new string[1] {node};
        }

        public bool IsSelected(string node)
        {
            node.VerifyArgument(nameof(node)).IsNotNull();

            return SelectedNodes.Contains(node);
        }

        public bool TryGetValue(string node, out object value) => Values.TryGetValue(node, out value);

        private IImmutableDictionary<string, IImmutableList<string>> Hierarchy { get; }
        private IImmutableDictionary<string, string> InverseHierarchy { get; }
        private IImmutableDictionary<string, object> Values { get; }
        private ImmutableHashSet<string> State { get; }

        public IEnumerable<string> TopLevelNodes => GetChildren(Root);
        public IEnumerable<string> SelectedNodes
        {
            get
            {
                var result = new List<string>();
                var currentParent = Root;
                while (Hierarchy.TryGetValue(currentParent, out var currentChildren))
                {
                    var lastParent = currentParent;
                    foreach (var child in currentChildren)
                    {
                        if (State.Contains(child))
                        {
                            result.Add(child);
                            currentParent = child;
                            break;
                        }
                    }

                    if (Comparer.Equals(lastParent, currentParent))
                    {
                        break;
                    }
                }
                return result;
            }
        }
    }
}