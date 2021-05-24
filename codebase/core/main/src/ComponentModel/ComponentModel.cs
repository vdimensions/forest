using System;
using System.Collections.Generic;
using Forest.Messaging;

namespace Forest.ComponentModel
{
    /// <summary>
    /// An interface for describing forest logical views.
    /// </summary>
    public interface IForestViewDescriptor
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
        
        /// <summary>
        /// A dictionary containing the <see cref="IForestCommandDescriptor">command descriptors</see> describing the
        /// commands defined by the represented <see cref="LogicalView">logical view</see>.
        /// </summary>
        IReadOnlyDictionary<string, IForestCommandDescriptor> Commands { get; }
        
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

        /// <summary>
        /// Gets or sets a value that indicates whether the view <see cref="Name">name</see>
        /// can be used to uniquely describe the target view.
        /// This essentially enables view composition in xml templates.
        /// </summary>
        bool TreatNameAsTypeAlias { get; }
    }
}
