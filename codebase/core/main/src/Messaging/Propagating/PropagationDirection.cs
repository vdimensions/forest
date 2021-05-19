using System;

namespace Forest.Messaging.Propagating
{
    [Flags]
    public enum PropagationDirection : byte
    {
        None = 0,
        Ancestors = 1,
        Descendants = 2,
        Siblings = 4
    }
}