using System;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Engine.Instructions
{
    /// <summary>
    /// An exception that is thrown when a logical view type does not implement the <see cref="IView{T}"/> interface.
    /// </summary>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class ViewTypeIsNotGenericException : ViewInstantiationException
    {
        private ViewTypeIsNotGenericException(InstantiateViewInstruction faultyInstruction, string message, Exception inner) 
            : base(faultyInstruction, message, inner) { }
        public ViewTypeIsNotGenericException(InstantiateViewInstruction faultyInstruction, Exception inner) 
            : this(faultyInstruction, "The view type must implement the Forest.IView<> interface. ", inner) { }
        public ViewTypeIsNotGenericException(InstantiateViewInstruction faultyInstruction) 
            : this(faultyInstruction, null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected ViewTypeIsNotGenericException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endif
    }
}