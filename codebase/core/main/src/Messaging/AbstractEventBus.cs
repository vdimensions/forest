using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Axle;
using Axle.Collections;
using Axle.Extensions.Object;

namespace Forest.Messaging
{
    internal abstract class AbstractEventBus : IEventBus
    {
        protected sealed class SubscriptionHandlerSet : HashSet<ISubscriptionHandler>
        {
            public SubscriptionHandlerSet() : base(new LensingEqualityComparer<ISubscriptionHandler, IView>(sh => sh.Receiver, new ReferenceEqualityComparer<IView>())) { }
        }

        [StructLayout(LayoutKind.Sequential)]
        protected readonly struct Letter : IComparable<Letter>, IEquatable<Letter>
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
                    hashCode = hashCode * -1521134295 + this.CalculateHashCode(Topics);
                    hashCode = hashCode * -1521134295 + Timestamp.GetHashCode();
                    return hashCode;
                }
            }

            public IView Sender { get; }
            public object Message { get; }
            public string[] Topics { get; }
            private long Timestamp { get; }
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