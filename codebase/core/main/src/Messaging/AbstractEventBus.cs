using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Axle;
using Axle.Collections;

namespace Forest.Messaging
{
    internal abstract class AbstractEventBus : IEventBus
    {
        protected sealed class SubscriptionHandlerSet : HashSet<ISubscriptionHandler>
        {
            public SubscriptionHandlerSet() : base(new LensingEqualityComparer<ISubscriptionHandler, IView>(sh => sh.Receiver, new ReferenceEqualityComparer<IView>())) { }
        }
        
        protected readonly IDictionary<Letter, SubscriptionHandlerSet> MessageHistory =
            new ChronologicalDictionary<Letter, SubscriptionHandlerSet>();
        
        protected static bool ReceiverIsSender(IView sender, ISubscriptionHandler subscription)
        {
            return ReferenceEquals(sender, subscription.Receiver);
        }

        protected static int InvokeMatchingSubscriptions(
            IView sender,
            object message,
            IDictionary<Type, SubscriptionHandlerSet> subscriptionHandlers,
            ISet<ISubscriptionHandler> subscribersToIgnore)
        {
            // Collect the event subscriptions before invocation. 
            // This is necessary, as some commands may cause view disposal and event unsubscription in result, 
            // which is undesired while iterating over the subscription collections
            var subscriptionsToCall =
                subscriptionHandlers
                    .Where(x => x.Key.GetTypeInfo().IsAssignableFrom(message.GetType().GetTypeInfo()))
                    .SelectMany(x => x.Value.Where(y => !ReceiverIsSender(sender, y)))
                    .Where(subscribersToIgnore.Add)
                    .ToList();
            // Now that we've collected all potential subscribers, it is safe to invoke them
            foreach (var s in subscriptionsToCall)
            {
                s.Invoke(message);
            }
            return subscriptionsToCall.Count;
        }
        
        private bool _processing;

        protected virtual void Dispose(bool _)
        {
            MessageHistory.Clear();
        }
        void IDisposable.Dispose() => Dispose(true);

        public virtual void Unsubscribe(IView receiver)
        {
            foreach (var historyKey in MessageHistory.Keys.Where(key => ReferenceEquals(key.Sender, receiver)).ToList())
            {
                MessageHistory.Remove(historyKey);
            }
        }

        protected abstract int ProcessMessage(Letter letter, ISet<ISubscriptionHandler> subscribersToIgnore);
        
        public void ProcessMessages()
        {
            if (_processing)
            {
                return;
            }
            _processing = true;
            try
            {
                int totalTopicMessagesProcessed;
                do
                {
                    totalTopicMessagesProcessed = MessageHistory.ToList().Sum(x => ProcessMessage(x.Key, x.Value));
                } 
                while (totalTopicMessagesProcessed > 0);
            }
            finally
            {
                _processing = false;
            }
        }

        public void ClearDeadLetters() => MessageHistory.Clear();
    }
}