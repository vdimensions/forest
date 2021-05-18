using System;

namespace Forest.Messaging
{
    public interface IEventDescriptor
    {
        void Trigger(IView view, object message);
        
        Type MessageType { get; }
    }
}