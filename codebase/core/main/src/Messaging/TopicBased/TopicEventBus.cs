using System;
using System.Collections.Generic;
using System.Linq;
using Axle.Verification;

namespace Forest.Messaging.TopicBased
{
    internal sealed class TopicEventBus : AbstractEventBus, ITopicEventBus
    {
        private readonly IDictionary<string, IDictionary<Type, SubscriptionHandlerSet>> _subscriptions =
            new Dictionary<string, IDictionary<Type, SubscriptionHandlerSet>>(StringComparer.Ordinal);

        protected override int ProcessMessage(Letter letter, ISet<ISubscriptionHandler> subscribersToIgnore)
        {
            var countHandled = 0;
            if (letter.DistributionData.Topics.Length == 0)
            {
                foreach (var topicSubscriptionHandlers in _subscriptions.Values.ToList())
                {
                    countHandled += InvokeMatchingSubscriptions(letter.Sender, letter.Message, topicSubscriptionHandlers, subscribersToIgnore);
                }
            }
            else
            {
                foreach (var topic in letter.DistributionData.Topics)
                {
                    if (_subscriptions.TryGetValue(topic, out var topicSubscriptionHandlers))
                    {
                        countHandled += InvokeMatchingSubscriptions(letter.Sender, letter.Message, topicSubscriptionHandlers, subscribersToIgnore);
                    }
                }
            }

            return countHandled;
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var value in _subscriptions.Values)
            {
                value.Clear();
            }
            _subscriptions.Clear();
            base.Dispose(disposing);
        }

        public void Publish<T>(IView sender, T message, params string[] topics)
        {
            message.VerifyArgument(nameof(message)).IsNotNull();
            var letter = new Letter(sender, message, DateTime.UtcNow.Ticks, new DistributionData(topics));
            if (!MessageHistory.TryGetValue(letter, out _))
            {
                MessageHistory[letter] = new SubscriptionHandlerSet();
            }
            ProcessMessages();
        }

        public void Subscribe(ISubscriptionHandler subscriptionHandler, string topic)
        {
            subscriptionHandler.VerifyArgument(nameof(subscriptionHandler)).IsNotNull();
            topic.VerifyArgument(nameof(topic)).IsNotNull();
            if (!_subscriptions.TryGetValue(topic, out var topicSubscriptionHandlers))
            {
                _subscriptions.Add(topic, topicSubscriptionHandlers = new Dictionary<Type, SubscriptionHandlerSet>());
            }

            if (!topicSubscriptionHandlers.TryGetValue(subscriptionHandler.MessageType, out var subscriptionSet))
            {
                topicSubscriptionHandlers.Add(subscriptionHandler.MessageType, subscriptionSet = new SubscriptionHandlerSet());
            }
            subscriptionSet.Add(subscriptionHandler);
        }
        
        public override void Unsubscribe(IView receiver)
        {
            foreach (var subscriptionHandlers in _subscriptions.Values.SelectMany(x => x.Values))
            foreach (var subscriptionHandler in subscriptionHandlers.Where(y => ReceiverIsSender(receiver, y)).ToList())
            {
                subscriptionHandlers.Remove(subscriptionHandler);
            }
            
            base.Unsubscribe(receiver);
        }
    }
}