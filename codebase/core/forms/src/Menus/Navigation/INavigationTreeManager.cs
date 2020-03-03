using System;

namespace Forest.Forms.Menus.Navigation
{
    public interface INavigationTreeManager : INotifyNavigationTreeChanged
    {
        void UpdateNavigationTree(Func<INavigationTreeBuilder, INavigationTreeBuilder> configure);
        
        NavigationTree NavigationTree { get; }
    }
}