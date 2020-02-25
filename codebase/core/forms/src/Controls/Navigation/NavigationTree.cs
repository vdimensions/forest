using System;
using System.Collections.Generic;

namespace Forest.Forms.Controls.Navigation
{
    public sealed class NavigationTree
    {
        private static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;
        internal const string Root = "";

        // TODO: use volatile tuple of immutable dictionaries instead
        private readonly IDictionary<string, string> _inverseHierarchy;
        private readonly IDictionary<string, HashSet<string>> _hierarchy;

        internal NavigationTree()
        {
            _inverseHierarchy = new Dictionary<string, string>(Comparer);
            _hierarchy = new Dictionary<string, HashSet<string>>(Comparer);
        }
        
        internal void RegisterNavigationTree(string current, string parent)
        {
            if (!_inverseHierarchy.ContainsKey(parent))
            {
                throw new ArgumentException(string.Format("Invalid navigation hierarchy: parent structure '{0}' was not found", parent));
            }
            if (!_inverseHierarchy.TryGetValue(current, out var existingParent))
            {
                _inverseHierarchy.Add(current, existingParent = parent);
            }
            else if (!Comparer.Equals(parent, existingParent))
            {
                throw new ArgumentException(
                    string.Format("Existing navigation structure '{1}'already contains the provided item '{0}'", current, existingParent));
            }
            if (!_hierarchy.TryGetValue(existingParent, out var children))
            {
                _hierarchy.Add(existingParent, children = new HashSet<string>(Comparer));
            }
            children.Add(current);
        }
    }
}