using System;
using Forest.Engine;

namespace Forest.Messaging
{
    internal sealed class EventHandler : ISubscriptionHandler
    {
        private readonly IEventDescriptor _descriptor;

        public EventHandler(IEventDescriptor descriptor, _ForestViewContext context, IView view)
        {
            _descriptor = descriptor;
            Context = context;
            Receiver = view;
        }
        public void Invoke(object message) => _descriptor.Trigger(Context, Receiver, message);

        public Type MessageType => _descriptor.MessageType;
        public IView Receiver { get; }
        public _ForestViewContext Context { get; }
    }
}