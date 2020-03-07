using System;
using Axle.Verification;

namespace Forest.Forms.Navigation
{
    internal sealed class NavigationTreeBuilder : INavigationTreeBuilder
    {
        private readonly string _parentNavigationItem;
        private readonly NavigationTree _navigationTree;

        public NavigationTreeBuilder(NavigationTree navigationTree) : this(navigationTree, NavigationTree.Root) { }
        public NavigationTreeBuilder(NavigationTree navigationTree, string parentNavigationItem)
        {
            _parentNavigationItem = parentNavigationItem;
            _navigationTree = navigationTree;
        }
        
        public NavigationTreeBuilder GetOrAddNode(
            string node, 
            object message,
            Func<INavigationTreeBuilder, INavigationTreeBuilder> buildNestedItems)
        {
            node.VerifyArgument(nameof(node)).IsNotNullOrEmpty();
            var nestedBuilder = new NavigationTreeBuilder(_navigationTree.RegisterNavigationNode(_parentNavigationItem, node, message), node);
            if (buildNestedItems != null)
            {
                nestedBuilder = (NavigationTreeBuilder) buildNestedItems(nestedBuilder);
            }
            return new NavigationTreeBuilder(nestedBuilder.Build(), _parentNavigationItem);
        }

        public NavigationTreeBuilder Remove(string node)
        {
            node.VerifyArgument(nameof(node)).IsNotNullOrEmpty();
            return new NavigationTreeBuilder(_navigationTree.UnregisterNavigationNode(node), _parentNavigationItem);
        }

        INavigationTreeBuilder INavigationTreeBuilder.GetOrAddNode(string node, Func<INavigationTreeBuilder, INavigationTreeBuilder> buildFn = null) 
            => GetOrAddNode(node, null, buildFn);
        INavigationTreeBuilder INavigationTreeBuilder.GetOrAddNode(string node, object message, Func<INavigationTreeBuilder, INavigationTreeBuilder> buildFn) 
            => GetOrAddNode(node, message, buildFn);

        INavigationTreeBuilder INavigationTreeBuilder.Remove(string node) 
            => Remove(node);

        public NavigationTree Build() => _navigationTree;
    }
}