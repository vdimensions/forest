using System;
using Forest.Engine;

namespace Forest.Messaging
{
    internal interface ISubscriptionHandler
    {
        void Invoke(object message);
        
        Type MessageType { get; }
        
        IView Receiver { get; }
        
        _ForestViewContext Context { get; }
    }
}