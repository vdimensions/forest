using System;
using Axle.Reflection;
using Forest.ComponentModel;

namespace Forest.Messaging.Propagating
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class PropagatingEventInvocationException : AbstractInvocationException
    {
        internal PropagatingEventInvocationException(Type viewType, IMethod method, Exception inner)
            : base(
                viewType, 
                method, 
                "Failed to invoke propagating event subscription. ", 
                inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        internal PropagatingEventInvocationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}