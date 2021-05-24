using System;
using Forest.Engine;

namespace Forest.Messaging
{
    internal interface IEventDescriptor
    {
        void Trigger(_ForestViewContext context, IView view, object message);
        
        Type MessageType { get; }
    }
}