using System.Collections.Generic;
using Forest.Messaging.Propagating;
using Forest.Messaging.TopicBased;

namespace Forest.ComponentModel
{
    internal interface _ForestViewDescriptor : IForestViewDescriptor
    {
        /// <summary>
        /// Gets a collection of <see cref="ITopicEventDescriptor">topic event descriptors</see> representing the event
        /// subscriptions that are defined by the represented <see cref="LogicalView">logical view</see>.
        /// </summary>
        IEnumerable<ITopicEventDescriptor> TopicEvents { get; }
        
        /// <summary>
        /// Gets a collection of <see cref="IPropagatingEventDescriptor">propagating event descriptors</see>
        /// representing the event subscriptions that are defined by the represented
        /// <see cref="LogicalView">logical view</see>.
        /// </summary>
        IEnumerable<IPropagatingEventDescriptor> PropagatingEvents { get; }
    }
}