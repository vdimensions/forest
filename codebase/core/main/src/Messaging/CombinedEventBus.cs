using System;
using Forest.Messaging.Propagating;
using Forest.Messaging.TopicBased;

namespace Forest.Messaging
{
    internal sealed class CombinedEventBus : ITopicEventBus, IPropagatingEventBus
    {
        private readonly ITopicEventBus _topicEventBus;
        private readonly IPropagatingEventBus _propagatingEventBus;

        public CombinedEventBus() : this(new TopicEventBus(), new PropagatingEventBus()) { }
        public CombinedEventBus(ITopicEventBus topicEventBus, IPropagatingEventBus propagatingEventBus)
        {
            _topicEventBus = topicEventBus;
            _propagatingEventBus = propagatingEventBus;
        }

        public void Dispose()
        {
            _topicEventBus.Dispose();
            _propagatingEventBus.Dispose();
        }
        void IDisposable.Dispose() => Dispose();

        public void Unsubscribe(IView receiver)
        {
            _topicEventBus.Unsubscribe(receiver);
            _propagatingEventBus.Unsubscribe(receiver);
        }

        public void ProcessMessages()
        {
            _topicEventBus.ProcessMessages();
            _propagatingEventBus.ProcessMessages();
        }

        public void ClearDeadLetters()
        {
            _topicEventBus.ClearDeadLetters();
            _propagatingEventBus.ClearDeadLetters();
        }

        public void Publish<TMessage>(IView sender, TMessage message, params string[] topics) => _topicEventBus.Publish(sender, message, topics);

        public void Subscribe(ISubscriptionHandler subscriptionHandler, string topic) => _topicEventBus.Subscribe(subscriptionHandler, topic);

        public void Propagate<TMessage>(IView sender, TMessage message) => _propagatingEventBus.Propagate(sender, message);

        public void Subscribe(ISubscriptionHandler subscriptionHandler) => _propagatingEventBus.Subscribe(subscriptionHandler);
    }
}