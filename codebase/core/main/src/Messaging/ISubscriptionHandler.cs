using System;

namespace Forest.Messaging
{
    internal interface ISubscriptionHandler
    {
        void Invoke(object message);
        
        Type MessageType { get; }
        
        IView Receiver { get; }
        
        Tree.Node Node { get; }
    }
}