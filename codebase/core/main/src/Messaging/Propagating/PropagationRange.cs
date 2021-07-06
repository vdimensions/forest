using System;

namespace Forest.Messaging.Propagating
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public enum PropagationRange : sbyte
    {
        None = 0,
        Minimum,
        Maximum
    }
}