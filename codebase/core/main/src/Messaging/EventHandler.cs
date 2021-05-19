using System;

namespace Forest.Messaging
{
    internal sealed class EventHandler : ISubscriptionHandler
    {
        private readonly IEventDescriptor _descriptor;

        public EventHandler(IEventDescriptor descriptor, IView view, Tree.Node node)
        {
            _descriptor = descriptor;
            Node = node;
            Receiver = view;
        }
        public void Invoke(object message) => _descriptor.Trigger(Receiver, message);

        public Type MessageType => _descriptor.MessageType;
        public IView Receiver { get; }
        public Tree.Node Node { get; }
    }
}