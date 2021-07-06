using System.Collections.Generic;
using Forest.Messaging.Propagating;
using Forest.Messaging.TopicBased;

namespace Forest.ComponentModel
{
    internal interface _ForestViewDescriptor : IForestViewDescriptor
    {
        IEnumerable<_TopicEventDescriptor> TopicEvents { get; }
        
        IEnumerable<_PropagatingEventDescriptor> PropagatingEvents { get; }
    }
}