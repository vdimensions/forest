using System;
using Forest.Navigation;

namespace Forest.ComponentModel
{
    public interface IForestCommandDescriptor
    {
        Location Invoke(IView sender, object arg);

        bool TryResolveRedirect(object arg, out Location redirect);
        
        /// <summary>
        /// Gets the name of the command
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the <see cref="Type"/> of the command argument,
        /// or <c>null</c> if the command does not accept any arguments.
        /// </summary>
        Type ArgumentType { get; }
    }
}
