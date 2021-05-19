using System;

namespace Forest.Messaging.Propagating
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [Flags]
    public enum PropagationDirection : byte
    {
        None = 0,
        Ancestors = 1,
        Descendants = 2,
        Siblings = 4
    }
}