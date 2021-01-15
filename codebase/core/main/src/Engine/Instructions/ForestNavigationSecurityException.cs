using System;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public class ForestNavigationSecurityException : ForestSecurityException
    {
        public ForestNavigationSecurityException() { }
        public ForestNavigationSecurityException(string message) : base(message) { }
        public ForestNavigationSecurityException(string message, Exception inner) : base(message, inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected ForestNavigationSecurityException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}