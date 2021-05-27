using System;

namespace Forest.Messaging
{
    public interface IEventDescriptor
    {
        Type MessageType { get; }
    }
}