using System;

namespace Forest.Navigation.Messages
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class NavigationTreeChanged
    {
        public NavigationTreeChanged(NavigationTree navigationTree)
        {
            NavigationTree = navigationTree;
        }

        public NavigationTree NavigationTree { get; }
    }
}