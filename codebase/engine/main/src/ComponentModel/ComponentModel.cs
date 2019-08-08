using System;
using System.Collections.Generic;

namespace Forest.ComponentModel
{
    public interface IViewDescriptor
    {
        string Name { get; }
        Type ViewType { get; }
        Type ModelType { get; }
        IReadOnlyDictionary<string, ILinkDescriptor> Links { get; }
        IReadOnlyDictionary<string, ICommandDescriptor> Commands { get; }
        IEnumerable<IEventDescriptor> Events { get; }
        bool IsSystemView { get; }
    }
}
