using Axle.Verification;

namespace Forest.Forms.Controls.Navigation
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

        public INavigationTreeBuilder RegisterNavigationTree(string navigationItem)
        {
            navigationItem.VerifyArgument(nameof(navigationItem)).IsNotNullOrEmpty();
            _navigationTree.RegisterNavigationTree(navigationItem, _parentNavigationItem);
            return new NavigationTreeBuilder(navigationItem, _navigationTree);
        }
    }
}