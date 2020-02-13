using System;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Engine.Instructions
{
    /// <summary>
    /// An exception that occurs when a logical view could not be instantiated.
    /// </summary>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public class ViewInstantiationException : ForestInstructionException
    {
        protected ViewInstantiationException(InstantiateViewInstruction faultyInstruction, string message, Exception inner) 
            : base(faultyInstruction, string.Format("An error occurred while instantiating view ({0}). {1}", faultyInstruction.Node.ViewHandle, message), inner) { }
        internal ViewInstantiationException(InstantiateViewInstruction faultyInstruction, Exception inner) 
            : this(faultyInstruction, string.Empty, inner) { }
        internal ViewInstantiationException(InstantiateViewInstruction faultyInstruction) 
            : this(faultyInstruction, null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected ViewInstantiationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endif
    }
}