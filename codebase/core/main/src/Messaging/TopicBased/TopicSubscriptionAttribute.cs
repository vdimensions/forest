using System;
using Axle.Verification;

namespace Forest.Messaging.TopicBased
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class TopicSubscriptionAttribute : SubscriptionAttribute
    {
        public TopicSubscriptionAttribute(string topic)
        {
            Topic = topic.VerifyArgument(nameof(topic)).IsNotNullOrEmpty();
        }
        public TopicSubscriptionAttribute()
        {
            Topic = string.Empty;
        }

        public string Topic { get; }
    }
}