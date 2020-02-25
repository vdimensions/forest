using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Axle.Modularity;
using Axle.Verification;
using Forest.ComponentModel;

namespace Forest.Forms.Controls.Navigation
{
    [Module]
    internal sealed class ForestNavigationSystemModule : IForestViewProvider, INavigationBuilder
    {
        private sealed class NavigationTreeBuilder : INavigationTreeBuilder
        {
            private readonly string _parentNavigationItem;
            private readonly Dictionary<string, string> _navigationTree;

            public NavigationTreeBuilder(string parentNavigationItem, Dictionary<string, string> navigationTree)
            {
                _parentNavigationItem = parentNavigationItem;
                _navigationTree = navigationTree;
            }

            public INavigationTreeBuilder RegisterNavigationTree(string navigationItem)
            {
                navigationItem.VerifyArgument(nameof(navigationItem)).IsNotNullOrEmpty();
                if (!_navigationTree.ContainsKey(_parentNavigationItem))
                {
                    throw new ArgumentException(string.Format("Invalid navigation hierarchy: parent structure '{0}' was not found", _parentNavigationItem));
                }
                if (_navigationTree.TryGetValue(navigationItem, out var existingParent))
                {
                    _navigationTree.Add(navigationItem, _parentNavigationItem);
                }
                else if (!_navigationTree.Comparer.Equals(_parentNavigationItem, existingParent))
                {
                    throw new ArgumentException(
                        string.Format("Existing navigation structure '{1}'already contains the provided item '{0}'", navigationItem, existingParent));
                }
                return new NavigationTreeBuilder(navigationItem, _navigationTree);
            }
        }

        private static readonly IEqualityComparer<string> NavigationTreeComparer = StringComparer.OrdinalIgnoreCase;
        
        private readonly Dictionary<string, string> _navigationTree = 
            new Dictionary<string, string>(NavigationTreeComparer)
            {
                {string.Empty, string.Empty}
            };
        
        public void RegisterViews(IViewRegistry registry)
        {
            registry
                .Register<NavigationSystem.View>()
                .Register<NavigationMenu.View>().Register<NavigationMenu.Item.View>()
                .Register<BreadcrumbsMenu.View>().Register<BreadcrumbsMenu.Item.View>()
                ;
        }

        [ModuleDependencyInitialized]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void DependencyInitialized(INavigationConfigurer navigationConfigurer) => 
            navigationConfigurer.Configure(this);

        public INavigationTreeBuilder RegisterNavigationTree(string navigationItem) => 
            new NavigationTreeBuilder(string.Empty, _navigationTree).RegisterNavigationTree(navigationItem);
    }
}
