using System;

namespace Forest.Forms.Navigation
{
    public interface INavigationTreeManager
    {
        void UpdateNavigationTree(Func<INavigationTreeBuilder, INavigationTreeBuilder> configure);
        
        NavigationTree NavigationTree { get; }
    }
}