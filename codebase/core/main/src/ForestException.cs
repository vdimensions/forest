using System;

namespace Forest
{
    /// <summary>
    /// A class to serve as a base for any forest exception.
    /// </summary>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public class ForestException : Exception
    {
        public ForestException() { }
        public ForestException(string message) : base(message) { }
        public ForestException(string message, Exception inner) : base(message, inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected ForestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}