using System;
using System.Collections.Generic;
using System.Linq;

namespace Forest.Events
{
    internal sealed class EventBus : IEventBus
    {
        [ThreadStatic]
        private static WeakReference _staticEventBus;

        public static EventBus Get()
        {
            var existing = _staticEventBus == null ? null : _staticEventBus.Target as EventBus;
            if (existing == null)
            {
                _staticEventBus = new WeakReference(existing = new EventBus());
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
                return (EventBus) wr.Target;
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
            DoDispose();
            var exiting = _staticEventBus.Target;
            if (ReferenceEquals(exiting, this))
            {
                _staticEventBus = null;
            }
        }
        private void DoDispose()
        {
            foreach (var value in subscriptions.Values)
            {
                value.Clear();
            }
            subscriptions.Clear();
        }

        public bool Publish<T>(IView sender, T message, string[] topics)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            var subscribersFound = 0;
            if (topics.Length > 0)
            {
                foreach (var topic in topics)
                {
                    IDictionary<Type, IList<ISubscriptionHandler>> topicSubscriptionHandlers;
                    if (subscriptions.TryGetValue(topic, out topicSubscriptionHandlers))
                    {
                        subscribersFound += InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers);
                    }
                }
            }
            else
            {
                foreach (var topicSubscriptionHandlers in subscriptions.Values)
                {
                    subscribersFound += InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers);
                }
            }
            
            return subscribersFound > 0;
        }

        private static int InvokeMatchingSubscriptions<T>(
            IView sender, 
            T message, 
            IDictionary<Type, IList<ISubscriptionHandler>> topicSubscriptionHandlers)
        {
            int subscribersFound = 0;
            var type = typeof (T);
            var keys = topicSubscriptionHandlers.Keys.Where(x => (type == x) || x.IsAssignableFrom(type));
            foreach (var subscription in keys.SelectMany(key => topicSubscriptionHandlers[key].Where(subscription => !ReferenceEquals(sender, subscription.Receiver))))
            {
                subscription.Invoke(message);
                subscribersFound++;
            }
            return subscribersFound;
        }

        public IEventBus Subscribe(ISubscriptionHandler subscriptionHandler, string topic)
        {
            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }
            if (subscriptionHandler == null)
            {
                throw new ArgumentNullException("subscriptionHandler");
            }

            IDictionary<Type, IList<ISubscriptionHandler>> topicSubscriptionHandlers;
            if (!subscriptions.TryGetValue(topic, out topicSubscriptionHandlers))
            {
                subscriptions.Add(topic, topicSubscriptionHandlers = new Dictionary<Type, IList<ISubscriptionHandler>>());
            }

            IList<ISubscriptionHandler> subscriptionList;
            if (!topicSubscriptionHandlers.TryGetValue(subscriptionHandler.MessageType, out subscriptionList))
            {
                topicSubscriptionHandlers.Add(subscriptionHandler.MessageType, subscriptionList = new List<ISubscriptionHandler>());
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
