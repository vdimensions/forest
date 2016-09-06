using System;
using System.Collections.Generic;
using System.Linq;

using Axle.Collections;
using Axle.Extensions.Type;
using Axle.Forest.UI.Composition;
using Axle.References;
using Axle.Verification;


namespace Axle.Forest.UI.Messaging
{
    class EventBus : IDisposable, IEventBus
    {
        [ThreadStatic]
        private static WeakReference<EventBus> _staticEventBus;

        public static EventBus Get()
        {
            var existing = _staticEventBus == null ? null : _staticEventBus.Value;
            if (existing == null)
            {
                _staticEventBus = new WeakReference<EventBus>(existing = new EventBus());
            }
            return existing.MarkUsed();
        }

        public static IEventBus Current
        {
            get
            {
                var wr = _staticEventBus;
                if ((wr == null) || !wr.IsAlive)
                {
                    throw new InvalidOperationException("No event bus is associated with the current thread!");
                }
                return wr.Value;
            }
        }

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private readonly IDictionary<string, IDictionary<Type, IList<ISubscriptionHandler>>> subscriptions;

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private volatile int usesCount = 0;

        private EventBus()
        {
            subscriptions = new Dictionary<string, IDictionary<Type, IList<ISubscriptionHandler>>>(StringComparer.Ordinal);
        }

        public EventBus MarkUsed()
        {
            usesCount++;
            return this;
        }

        public void Dispose()
        {
            if (--usesCount > 0)
            {
                return;
            }
            Dispose(true);
            var exiting = _staticEventBus.Target;
            if (ReferenceEquals(exiting, this))
            {
                _staticEventBus = null;
            }
        }
        private void Dispose(bool disposing)
        {
            foreach (var value in subscriptions.Values)
            {
                value.Clear();
            }
            subscriptions.Clear();
        }

        public bool Publish<T>(IView sender, T message, string topic)
        {
            topic.VerifyArgument("topic").IsNotNull();
            message.VerifyArgument("message").IsNotNull();
            IDictionary<Type, IList<ISubscriptionHandler>> topicSubscriptionHandlers;
            var subscribersFound = 0;
            if (subscriptions.TryGetValue(topic, out topicSubscriptionHandlers))
            {
                var type = typeof(T);
                var keys = topicSubscriptionHandlers.Keys.Where(x => (type == x) || type.ExtendsOrImplements(x));
                foreach (var subscription in keys.SelectMany(key => topicSubscriptionHandlers[key].Where(subscription => !ReferenceEquals(sender, subscription.Receiver))))
                {
                    subscription.Invoke(message);
                    subscribersFound++;
                }
            }
            
            return subscribersFound > 0;
        }

        public IEventBus Subscribe(ISubscriptionHandler subscriptionHandler, string topic)
        {
            topic.VerifyArgument("topic").IsNotNull();
            subscriptionHandler.VerifyArgument("subscriptionHandler").IsNotNull();

            IDictionary<Type, IList<ISubscriptionHandler>> topicSubscriptionHandlers;
            if (!subscriptions.TryGetValue(topic, out topicSubscriptionHandlers))
            {
                subscriptions.Add(topic, topicSubscriptionHandlers = new Dictionary<Type, IList<ISubscriptionHandler>>());
            }

            IList<ISubscriptionHandler> subscriptionList;
            if (!topicSubscriptionHandlers.TryGetValue(subscriptionHandler.MessageType, out subscriptionList))
            {
                topicSubscriptionHandlers.Add(subscriptionHandler.MessageType, subscriptionList = new ArrayList<ISubscriptionHandler>());
            }
            subscriptionList.Add(subscriptionHandler); 
            
            return this;
        }

        public IEventBus Unsubscribe(IView receiver)
        {
            foreach (var topicSubscriptionHandlers in subscriptions.Values.SelectMany(x => x.Values))
            {
                foreach (var subscriptionHandler in topicSubscriptionHandlers.Where(x => ReferenceEquals(x.Receiver, receiver)).ToArray())
                {
                    topicSubscriptionHandlers.Remove(subscriptionHandler);
                }
            }
            return this;
        }
    }
}
