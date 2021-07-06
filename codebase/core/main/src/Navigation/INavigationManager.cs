using System;

namespace Forest.Navigation
{
    public interface INavigationManager
    {
        void UpdateNavigationTree(Func<INavigationTreeBuilder, INavigationTreeBuilder> configure);
        
        NavigationTree NavigationTree { get; }
    }
}