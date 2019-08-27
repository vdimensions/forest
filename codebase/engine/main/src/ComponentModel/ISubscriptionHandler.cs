using System;

namespace Forest.ComponentModel
{
    internal interface ISubscriptionHandler
    {
        void Invoke(object message);
        Type MessageType{ get; }
        IView Receiver { get; }
    }
}