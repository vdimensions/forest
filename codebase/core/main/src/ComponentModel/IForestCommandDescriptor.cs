using System;
using Forest.Navigation;

namespace Forest.ComponentModel
{
    public interface IForestCommandDescriptor
    {
        Location Invoke(IView sender, object arg);
        string Name { get; }
        Type ArgumentType { get; }
    }
}
