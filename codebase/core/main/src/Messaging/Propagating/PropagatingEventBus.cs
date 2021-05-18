using System;
using System.Collections.Generic;
using System.Linq;
using Axle.Verification;

namespace Forest.Messaging.Propagating
{
    internal sealed class PropagatingEventBus : AbstractEventBus, IPropagatingEventBus
    {
        private readonly IDictionary<Type, SubscriptionHandlerSet> _subscriptions 
            = new Dictionary<Type, SubscriptionHandlerSet>();

        protected override int ProcessMessage(Letter letter, ISet<ISubscriptionHandler> subscribersToIgnore)
        {
            return InvokeMatchingSubscriptions(letter.Sender, letter.Message, _subscriptions, subscribersToIgnore);
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

        public void Propagate<T>(IView sender, T message)
        {
            message.VerifyArgument(nameof(message)).IsNotNull();
            var letter = new Letter(sender, message, DateTime.UtcNow.Ticks);
            if (!MessageHistory.TryGetValue(letter, out _))
            {
                MessageHistory[letter] = new SubscriptionHandlerSet();
            }
            ProcessMessages();
        }
        
        public void Subscribe(ISubscriptionHandler subscriptionHandler)
        {
            subscriptionHandler.VerifyArgument(nameof(subscriptionHandler)).IsNotNull();
            if (!_subscriptions.TryGetValue(subscriptionHandler.MessageType, out var subscriptionSet))
            {
                _subscriptions.Add(subscriptionHandler.MessageType, subscriptionSet = new SubscriptionHandlerSet());
            }
            subscriptionSet.Add(subscriptionHandler);
        }

        public override void Unsubscribe(IView receiver)
        {
            foreach (var topicSubscriptionHandlers in _subscriptions.Values)
            foreach (var subscriptionHandler in topicSubscriptionHandlers.Where(y => ReceiverIsSender(receiver, y)).ToList())
            {
                topicSubscriptionHandlers.Remove(subscriptionHandler);
            }
            base.Unsubscribe(receiver);
        }
    }
}