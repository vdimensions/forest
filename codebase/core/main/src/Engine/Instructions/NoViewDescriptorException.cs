using System;

namespace Forest.Engine.Instructions
{
    /// <summary>
    /// An exception that is thrown when a view descriptor cannot be obtained for a given view handle.
    /// </summary>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class NoViewDescriptorException : ViewInstantiationException
    {
        private NoViewDescriptorException(InstantiateViewInstruction faultyInstruction, string message, Exception inner) 
            : base(faultyInstruction, message, inner) { }
        internal NoViewDescriptorException(InstantiateViewInstruction faultyInstruction, Exception inner) 
            : this(faultyInstruction, "Unable to obtain view descriptor. ", inner) { }
        internal NoViewDescriptorException(InstantiateViewInstruction faultyInstruction) 
            : this(faultyInstruction, null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        internal NoViewDescriptorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}