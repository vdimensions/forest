using System;
using System.Runtime.Serialization;

namespace Forest.EventSystem
{
    [Serializable]
    public class SubscriptionExecutionException : Exception
    {
        public SubscriptionExecutionException() { }
        public SubscriptionExecutionException(string message) : base(message) { }
        public SubscriptionExecutionException(string message, Exception inner) : base(message, inner) { }

        protected SubscriptionExecutionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}