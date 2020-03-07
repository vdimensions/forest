using System;

namespace Forest.Forms.Navigation
{
    public interface INotifyNavigationTreeChanged
    {
        event Action<NavigationTree> NavigationTreeChanged;
    }
}