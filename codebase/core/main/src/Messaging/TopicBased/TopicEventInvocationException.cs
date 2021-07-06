using System;
using Axle.Reflection;
using Axle.Verification;
using Forest.ComponentModel;

namespace Forest.Messaging.TopicBased
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class TopicEventInvocationException : AbstractInvocationException
    {
        internal TopicEventInvocationException(Type viewType, IMethod method, string topic, Exception inner)
            : base(
                viewType, 
                method, 
                string.Format("Failed to invoke event subscription for topic '{0}'. ", topic.VerifyArgument(nameof(topic)).IsNotNull().Value), 
                inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        internal TopicEventInvocationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}