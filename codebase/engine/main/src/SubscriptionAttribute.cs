using System;
using Axle.Verification;

namespace Forest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class SubscriptionAttribute : Attribute
    {
        public SubscriptionAttribute(string topic)
        {
            Topic = topic.VerifyArgument(nameof(topic)).IsNotNullOrEmpty();
        }
        public SubscriptionAttribute()
        {
            Topic = string.Empty;
        }

        public string Topic { get; }
    }
}