/**
 * Copyright 2014 vdimensions.net.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;


namespace Forest.Events
{
    using ISubscriptionCollection = IDictionary<Type, ICollection<ISubscriptionHandler>>;
    using ITopicSubscriptionCollection = IDictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>>;
    using SubscriptionCollection = Dictionary<Type, ICollection<ISubscriptionHandler>>;
    using TopicSubscriptionCollection = Dictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>>;

    internal sealed class EventBus : IEventBus
    {
        [ThreadStatic]
        private static WeakReference _staticEventBus;

        public static EventBus Get()
        {
            var existing = _staticEventBus?.Target as EventBus;
            if (existing == null)
            {
                _staticEventBus = new WeakReference(existing = new EventBus());
            }
            return existing.IncreaseUsageCount();
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
        private readonly ITopicSubscriptionCollection _subscriptions;

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private volatile int _usageCount = 0;

        private EventBus()
        {
            _subscriptions = new TopicSubscriptionCollection(StringComparer.Ordinal);
        }

        private EventBus IncreaseUsageCount()
        {
            _usageCount++;
            return this;
        }

        public void Dispose()
        {
            if (--_usageCount > 0)
            {
                return;
            }

            foreach (var value in _subscriptions.Values)
            {
                value.Clear();
            }
            _subscriptions.Clear();

            var exiting = _staticEventBus.Target;
            if (ReferenceEquals(exiting, this))
            {
                _staticEventBus = null;
            }
        }
        void IDisposable.Dispose() { Dispose(); }

        public bool Publish<T>(IView sender, T message, string[] topics)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            var subscribersFound = 0;
            if (topics.Length > 0)
            {
                for (var i = 0; i < topics.Length; i++)
                {
                    if (_subscriptions.TryGetValue(topics[i], out var topicSubscriptionHandlers))
                    {
                        subscribersFound += InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers);
                    }
                }
            }
            else
            {
                foreach (var topicSubscriptionHandlers in _subscriptions.Values)
                {
                    subscribersFound += InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers);
                }
            }
            
            return subscribersFound > 0;
        }

        private static int InvokeMatchingSubscriptions<T>(IView sender, T message, ISubscriptionCollection topicSubscriptionHandlers)
        {
            var subscribersFound = 0;
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
                throw new ArgumentNullException(nameof(topic));
            }
            if (subscriptionHandler == null)
            {
                throw new ArgumentNullException(nameof(subscriptionHandler));
            }

            if (!_subscriptions.TryGetValue(topic, out var topicSubscriptionHandlers))
            {
                _subscriptions.Add(topic, topicSubscriptionHandlers = new SubscriptionCollection());
            }

            if (!topicSubscriptionHandlers.TryGetValue(subscriptionHandler.MessageType, out var subscriptionList))
            {
                topicSubscriptionHandlers.Add(subscriptionHandler.MessageType, subscriptionList = new List<ISubscriptionHandler>());
            }
            subscriptionList.Add(subscriptionHandler); 
            
            return this;
        }

        public IEventBus Unsubscribe(IView receiver)
        {
            foreach (var topicSubscriptionHandlers in _subscriptions.Values.SelectMany(x => x.Values))
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
