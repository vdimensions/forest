using System;

namespace Forest.Forms.Navigation
{
    internal sealed partial class NavigationSystemModule : INavigationTreeManager
    {
        public void UpdateNavigationTree(Func<INavigationTreeBuilder, INavigationTreeBuilder> configure)
        {
            var inputBuilder = new NavigationTreeBuilder(NavigationTree, NavigationTree.Root);//new DelegatingNavigationTreeBuilder(this);
            var outputBuilder = configure(inputBuilder);
            var result = ((NavigationTreeBuilder) outputBuilder).Build();
            _navigationTree = result;
            NavigationTreeChanged?.Invoke(result);
        }

        public event Action<NavigationTree> NavigationTreeChanged;

        public NavigationTree NavigationTree => _navigationTree;
    }
}