using System;

namespace Forest.Forms.Menus.Navigation
{
    public interface INotifyNavigationTreeChanged
    {
        event Action<NavigationTree> NavigationTreeChanged;
    }
}