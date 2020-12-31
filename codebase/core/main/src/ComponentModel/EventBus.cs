using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Axle;
using Axle.Collections;
using Axle.Extensions.Object;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal sealed class EventBus : IEventBus
    {
        internal sealed class SubscriptionHandlerSet : HashSet<ISubscriptionHandler>
        {
            public SubscriptionHandlerSet() : base(new LensingEqualityComparer<ISubscriptionHandler, IView>(sh => sh.Receiver, new ReferenceEqualityComparer<IView>())) { }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Letter : IComparable<Letter>, IEquatable<Letter>
        {
            public Letter(IView sender, object message, long timestamp, params string[] topics)
            {
                Sender = sender;
                Message = message;
                Timestamp = timestamp;
                Topics = topics;
            }

            public int CompareTo(Letter other) => Timestamp.CompareTo(other.Timestamp);

            public override bool Equals(object obj) => obj is Letter other && Equals(other);

            public bool Equals(Letter other)
            {
                return ReferenceEquals(Sender, other.Sender) && Equals(Message, other.Message) && Topics.SequenceEqual(other.Topics) && Timestamp == other.Timestamp;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = 848193896;
                    hashCode = hashCode * -1521134295 + (Sender == null ? 0 : Sender.GetHashCode());
                    hashCode = hashCode * -1521134295 + Message.GetHashCode();
                    hashCode = hashCode * this.CalculateHashCode(-1521134295, (object[]) Topics);
                    hashCode = hashCode * -1521134295 + Timestamp.GetHashCode();
                    return hashCode;
                }
            }

            public IView Sender { get; }
            public object Message { get; }
            public string[] Topics { get; }
            private long Timestamp { get; }
        }

        private readonly IDictionary<string, IDictionary<Type, SubscriptionHandlerSet>> _subscriptions =
            new Dictionary<string, IDictionary<Type, SubscriptionHandlerSet>>(StringComparer.Ordinal);

        private readonly IDictionary<Letter, SubscriptionHandlerSet> _messageHistory =
            new ChronologicalDictionary<Letter, SubscriptionHandlerSet>();

        private bool _processing;

        private static bool ReceiverIsSender(IView sender, ISubscriptionHandler subscription)
        {
            return ReferenceEquals(sender, subscription.Receiver);
        }

        private int InvokeMatchingSubscriptions(
            IView sender,
            object message,
            IDictionary<Type, SubscriptionHandlerSet> topicSubscriptionHandlers,
            ISet<ISubscriptionHandler> subscribersToIgnore)
        {
            // Collect the event subscriptions before invocation. 
            // This is necessary, as some commands may cause view disposal and event unsubscription in result, 
            // which is undesired while iterating over the subscription collections
            var subscriptionsToCall =
                topicSubscriptionHandlers
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

        private int DoPublish(Letter letter, ISet<ISubscriptionHandler> subscribersToIgnore)
        {
            var countHandled = 0;
            if (letter.Topics.Length == 0)
            {
                foreach (var topicSubscriptionHandlers in _subscriptions.Values.ToList())
                {
                    countHandled += InvokeMatchingSubscriptions(letter.Sender, letter.Message, topicSubscriptionHandlers, subscribersToIgnore);
                }
            }
            else
            {
                foreach (var topic in letter.Topics)
                {
                    if (_subscriptions.TryGetValue(topic, out var topicSubscriptionHandlers))
                    {
                        countHandled += InvokeMatchingSubscriptions(letter.Sender, letter.Message, topicSubscriptionHandlers, subscribersToIgnore);
                    }
                }
            }

            return countHandled;
        }

        void IDisposable.Dispose()
        {
            foreach (var value in _subscriptions.Values)
            {
                value.Clear();
            }
            _subscriptions.Clear();
            _messageHistory.Clear();
        }

        public void Publish<T>(IView sender, T message, params string[] topics)
        {
            message.VerifyArgument(nameof(message)).IsNotNull();
            var letter = new Letter(sender, message, DateTime.UtcNow.Ticks, topics);
            if (!_messageHistory.TryGetValue(letter, out _))
            {
                _messageHistory[letter] = new SubscriptionHandlerSet();
            }
            ProcessMessages();
        }

        public IEventBus Subscribe(ISubscriptionHandler subscriptionHandler, string topic)
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
            return this;
        }

        public IEventBus ProcessMessages()
        {
            if (_processing)
            {
                return this;
            }
            _processing = true;
            try
            {
                var totalMessagesProcessed = 0;
                do
                {
                    totalMessagesProcessed = _messageHistory.ToList().Sum(x => DoPublish(x.Key, x.Value));
                } 
                while (totalMessagesProcessed > 0);
            }
            finally
            {
                _processing = false;
            }

            return this;
        }

        public IEventBus Unsubscribe(IView receiver)
        {
            foreach (var topicSubscriptionHandlers in _subscriptions.Values.SelectMany(x => x.Values))
            foreach (var subscriptionHandler in topicSubscriptionHandlers.Where(y => ReceiverIsSender(receiver, y)).ToList())
            {
                topicSubscriptionHandlers.Remove(subscriptionHandler);
            }
            
            foreach (var historyKey in _messageHistory.Keys.Where(key => ReferenceEquals(key.Sender, receiver)).ToList())
            {
                _messageHistory.Remove(historyKey);
            }

            return this;
        }

        public IEventBus ClearDeadLetters()
        {
            _messageHistory.Clear();
            return this;
        }
    }
}