using System;

namespace Forest.Forms.Navigation
{
    internal sealed partial class NavigationSystemModule : INavigationTreeManager
    {
        void INavigationTreeManager.UpdateNavigationTree(Func<INavigationTreeBuilder, INavigationTreeBuilder> configure)
        {
            var inputBuilder = new NavigationTreeBuilder(_navigationTree, NavigationTree.Root);
            var outputBuilder = configure(inputBuilder);
            var result = ((NavigationTreeBuilder) outputBuilder).Build();
            _navigationTree = result;
        }

        public NavigationTree NavigationTree => _navigationTree;
    }
}