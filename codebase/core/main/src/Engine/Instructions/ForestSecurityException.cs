using System;
using System.Security;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public class ForestSecurityException : SecurityException
    {
        public ForestSecurityException() { }
        public ForestSecurityException(string message) : base(message) { }
        public ForestSecurityException(string message, Exception inner) : base(message, inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected ForestSecurityException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}