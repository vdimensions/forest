using System;

namespace Forest.Messaging
{
    internal sealed class EventHandler : ISubscriptionHandler
    {
        private readonly IEventDescriptor _descriptor;

        public EventHandler(IEventDescriptor descriptor, IView view)
        {
            _descriptor = descriptor;
            Receiver = view;
        }
        public void Invoke(object message) => _descriptor.Trigger(Receiver, message);

        public Type MessageType => _descriptor.MessageType;
        public IView Receiver { get; }
    }
}