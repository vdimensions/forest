using Axle.Verification;

namespace Forest.Forms.Menus.Navigation
{
    internal sealed class NavigationTreeBuilder : INavigationTreeBuilder
    {
        private readonly string _parentNavigationItem;
        private readonly NavigationTree _navigationTree;

        public NavigationTreeBuilder(string parentNavigationItem, NavigationTree navigationTree)
        {
            _parentNavigationItem = parentNavigationItem;
            _navigationTree = navigationTree;
        }
        
        public NavigationTreeBuilder GetOrAddNode(string node, object message)
        {
            node.VerifyArgument(nameof(node)).IsNotNullOrEmpty();
            return new NavigationTreeBuilder(
                node, 
                _navigationTree.RegisterNavigationNode(node, _parentNavigationItem, message));
        }

        public NavigationTreeBuilder Remove(string node)
        {
            node.VerifyArgument(nameof(node)).IsNotNullOrEmpty();
            return new NavigationTreeBuilder(_parentNavigationItem, _navigationTree.UnregisterNavigationNode(node));
        }

        public NavigationTreeBuilder Toggle(string node, bool selected)
        {
            node.VerifyArgument(nameof(node)).IsNotNullOrEmpty();
            return new NavigationTreeBuilder(_parentNavigationItem, _navigationTree.ToggleNode(node, selected));
        }

        INavigationTreeBuilder INavigationTreeBuilder.GetOrAddNode(string node, object message) 
            => GetOrAddNode(node, message);

        INavigationTreeBuilder INavigationTreeBuilder.Remove(string node) 
            => Remove(node);

        INavigationTreeBuilder INavigationTreeBuilder.Toggle(string node, bool selected) 
            => Toggle(node, selected);

        public NavigationTree Build() => _navigationTree;
    }
}