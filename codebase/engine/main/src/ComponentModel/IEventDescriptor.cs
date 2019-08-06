using System;

namespace Forest.ComponentModel
{
    public interface IEventDescriptor
    {
        void Trigger(IView view, object message);
        string Topic { get; }
        Type MessageType { get; }
    }
}
