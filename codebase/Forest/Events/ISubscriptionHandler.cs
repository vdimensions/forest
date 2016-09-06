using System;

namespace Forest.Events
{
    public interface ISubscriptionHandler
    {
        void Invoke(object arg);

        Type MessageType { get; }
        IView Receiver { get; }
    }
}