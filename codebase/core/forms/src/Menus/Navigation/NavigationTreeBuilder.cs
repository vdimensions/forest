using Axle.Verification;

namespace Forest.Forms.Menus.Navigation
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
        
        public NavigationTreeBuilder GetOrAddNode(string node, object message)
        {
            node.VerifyArgument(nameof(node)).IsNotNullOrEmpty();
            return new NavigationTreeBuilder(_navigationTree.RegisterNavigationNode(_parentNavigationItem, node, message), node);
        }

        public NavigationTreeBuilder Remove(string node)
        {
            node.VerifyArgument(nameof(node)).IsNotNullOrEmpty();
            return new NavigationTreeBuilder(_navigationTree.UnregisterNavigationNode(node), _parentNavigationItem);
        }

        INavigationTreeBuilder INavigationTreeBuilder.GetOrAddNode(string node, object message) 
            => GetOrAddNode(node, message);

        INavigationTreeBuilder INavigationTreeBuilder.Remove(string node) 
            => Remove(node);

        public NavigationTree Build() => _navigationTree;
    }
}