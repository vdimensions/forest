using System;
using System.Collections.Generic;

namespace Forest.ComponentModel
{
    /// <summary>
    /// An interface for describing forest logical views.
    /// </summary>
    public interface IViewDescriptor
    {
        /// <summary>
        /// Gets the name of the described <see cref="LogicalView">logical view</see>. 
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the type of the represented <see cref="LogicalView"/> implementation.
        /// </summary>
        Type ViewType { get; }
        /// <summary>
        /// Gets the type of the model object for the represented <see cref="LogicalView">logical view</see>.
        /// </summary>
        Type ModelType { get; }
        
        [Obsolete]
        IReadOnlyDictionary<string, ILinkDescriptor> Links { get; }
        
        /// <summary>
        /// A dictionary containing the <see cref="ICommandDescriptor">command descriptors</see> describing the
        /// commands defined by the represented <see cref="LogicalView">logical view</see>.
        /// </summary>
        IReadOnlyDictionary<string, ICommandDescriptor> Commands { get; }
        /// <summary>
        /// Gets a collection of <see cref="IEventDescriptor">event descriptors</see> representing the event
        /// subscriptions that are defined by the represented <see cref="LogicalView">logical view</see>.
        /// </summary>
        IEnumerable<IEventDescriptor> Events { get; }
        /// <summary>
        /// Gets a <see cref="bool"/> value indicating whether the represented
        /// <see cref="LogicalView">logical view</see> is a <see cref="IsSystemView">system view</see>
        /// </summary>
        /// <seealso cref="IsSystemView"/>
        bool IsSystemView { get; }
        /// <summary>
        /// Gets a <see cref="bool"/> value indicating whether the represented
        /// <see cref="LogicalView">logical view</see> is an anonymous view.
        /// </summary>
        bool IsAnonymousView { get; }
    }
}
