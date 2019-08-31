using System;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Engine.Instructions
{
    /// <summary>
    /// An exception that is thrown when the logical view type is abstract.
    /// </summary>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class ViewTypeIsAbstractException : ViewInstantiationException
    {
        private ViewTypeIsAbstractException(InstantiateViewInstruction faultyInstruction, string message, Exception inner) 
            : base(faultyInstruction, message, inner) { }
        public ViewTypeIsAbstractException(InstantiateViewInstruction faultyInstruction, Exception inner) 
            : this(faultyInstruction, "The view type is abstract. ", inner) { }
        public ViewTypeIsAbstractException(InstantiateViewInstruction faultyInstruction) 
            : this(faultyInstruction, null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected ViewTypeIsAbstractException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endif
    }
}