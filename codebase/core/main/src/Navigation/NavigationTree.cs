using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Axle.Collections.Immutable;
using Axle.Verification;

namespace Forest.Navigation
{
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NavigationTree
    {
        private static ICollection<string> ExpandChildren(
            string node, 
            ImmutableDictionary<string, ImmutableList<string>> hierarchy, 
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
            ImmutableDictionary<string, string> inverseHierarchy, 
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
        internal const string Root = "";

        public NavigationTree() 
            : this(
                ImmutableDictionary.Create<string, ImmutableList<string>>(Comparer),
                ImmutableDictionary.Create<string, string>(Comparer),
                ImmutableHashSet.Create(Comparer)) { }
        private NavigationTree(
            ImmutableDictionary<string, ImmutableList<string>> hierarchy, 
            ImmutableDictionary<string, string> inverseHierarchy, 
            ImmutableHashSet<string> state)
        {
            Hierarchy = hierarchy;
            InverseHierarchy = inverseHierarchy;
            State = state;
        }

        public NavigationTree RegisterNavigationNode(string parent, string current, object message = null)
        {
            parent.VerifyArgument(nameof(parent)).IsNotNull();
            current.VerifyArgument(nameof(current)).IsNotNull();
            
            var inverseHierarchy = InverseHierarchy;
            var hierarchy = Hierarchy;
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

            return new NavigationTree(hierarchy, inverseHierarchy, selectedState);
        }
        
        public NavigationTree UnregisterNavigationNode(string node)
        {
            node.VerifyArgument(nameof(node)).IsNotNull();
            
            var inverseHierarchy = InverseHierarchy;
            var hierarchy = Hierarchy;
            var selectedState = State;
            
            if (inverseHierarchy.TryGetValue(node, out var parentNode) && hierarchy.TryGetValue(parentNode, out var siblings))
            {
                hierarchy = hierarchy.Remove(parentNode).Add(parentNode, siblings.Remove(node));
            }

            foreach (var childToRemove in ExpandChildren(node, Hierarchy, new HashSet<string>(Comparer)))
            {
                hierarchy = hierarchy.Remove(childToRemove);
                inverseHierarchy = inverseHierarchy.Remove(childToRemove);
            }
            
            return new NavigationTree(hierarchy, inverseHierarchy, selectedState);
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
            var selectedState = State.Clear();

            if (selected)
            {
                var ancestors = ExpandAncestors(node, inverseHierarchy, new HashSet<string>(Comparer));
                foreach (var ancestor in ancestors)
                {
                    selectedState = selectedState.Add(ancestor);
                }
            }
            
            tree = new NavigationTree(hierarchy, inverseHierarchy, selectedState.Remove(Root));
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

        private ImmutableDictionary<string, ImmutableList<string>> Hierarchy { get; }
        private ImmutableDictionary<string, string> InverseHierarchy { get; }
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