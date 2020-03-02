using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Axle.Verification;

namespace Forest.Forms.Menus.Navigation
{
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NavigationTree
    {
        private static ICollection<string> ExpandChildren(
            string node, 
            IImmutableDictionary<string, ImmutableHashSet<string>> hierarchy, 
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
                ExpandAncestors(parent, inverseHierarchy, ancestors).Add(parent);
            }
            ancestors.Add(node);
            return ancestors;
        }

        private static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;
        internal const string Root = "";

        public NavigationTree() 
            : this(
                ImmutableDictionary.Create<string, ImmutableHashSet<string>>(Comparer),
                ImmutableDictionary.Create<string, string>(Comparer),
                ImmutableDictionary.Create<string, object>(Comparer), 
                ImmutableHashSet.Create(Comparer)) { }
        private NavigationTree(
            IImmutableDictionary<string, ImmutableHashSet<string>> hierarchy, 
            IImmutableDictionary<string, string> inverseHierarchy, 
            IImmutableDictionary<string, object> messageData, 
            ImmutableHashSet<string> selectedState)
        {
            Hierarchy = hierarchy;
            InverseHierarchy = inverseHierarchy;
            MessageData = messageData;
            SelectedState = selectedState;
        }

        public NavigationTree RegisterNavigationNode(string parent, string current, object message = null)
        {
            parent.VerifyArgument(nameof(parent)).IsNotNull();
            current.VerifyArgument(nameof(current)).IsNotNull();
            
            var inverseHierarchy = InverseHierarchy;
            var hierarchy = Hierarchy;
            var messageData = MessageData;
            var selectedState = SelectedState;
            if (!inverseHierarchy.ContainsKey(parent))
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
                children = ImmutableHashSet.Create(Comparer);
            }
            hierarchy = hierarchy.Remove(existingParent).Add(existingParent, children.Add(current));

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
            var messageData = MessageData;
            var selectedState = SelectedState;
            
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

        public NavigationTree ToggleNode(string node, bool selected)
        {
            node.VerifyArgument(nameof(node)).IsNotNull();
            
            if (selected == SelectedState.Contains(node))
            {
                return this;
            }
            
            var inverseHierarchy = InverseHierarchy;
            var hierarchy = Hierarchy;
            var messageData = MessageData;
            var selectedState = SelectedState.Clear();

            if (selected)
            {
                var ancestors = ExpandAncestors(node, inverseHierarchy, new HashSet<string>(Comparer));
                foreach (var ancestor in ancestors)
                {
                    selectedState = selectedState.Add(ancestor);
                }
            }
            
            return new NavigationTree(hierarchy, inverseHierarchy, messageData, selectedState);
        }

        private IImmutableDictionary<string, ImmutableHashSet<string>> Hierarchy { get; }
        private IImmutableDictionary<string, string> InverseHierarchy { get; }
        private IImmutableDictionary<string, object> MessageData { get; }
        private ImmutableHashSet<string> SelectedState { get; }

        public IEnumerable<string> TopLevelNodes => Hierarchy.TryGetValue(Root, out var result) 
                ? result as IEnumerable<string> 
                : new string[0];
        public IEnumerable<string> SelectedNodes
        {
            get
            {
                var result = new List<string>();
                var currentParent = Root;
                while (Hierarchy.TryGetValue(currentParent, out var currentChildren))
                {
                    foreach (var child in currentChildren)
                    {
                        if (SelectedState.Contains(child))
                        {
                            result.Add(child);
                            currentParent = child;
                            break;
                        }
                    }
                }
                return result;
            }
        }
    }
}