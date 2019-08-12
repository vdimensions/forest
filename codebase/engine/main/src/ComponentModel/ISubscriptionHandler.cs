using System;

namespace Forest.ComponentModel
{
    internal interface ISubscriptionHandler
    {
        void Invoke(object arg);
        Type MessageType{ get; }
        IView Receiver { get; }
    }
}