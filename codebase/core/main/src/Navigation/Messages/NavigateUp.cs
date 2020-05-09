using System;

namespace Forest.Navigation.Messages
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    internal struct NavigateUp { }
}