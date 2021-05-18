using System;

namespace Forest.Messaging.Propagating
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class PropagatingSubscriptionAttribute : SubscriptionAttribute
    {
    }
}