using System;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Commands.Instructions
{
    /// <summary>
    /// An exception that occurs when a command is being invoked.
    /// </summary>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class CommandSourceNotFoundException : CommandInstructionException
    {
        internal CommandSourceNotFoundException(InvokeCommandInstruction faultyInstruction, Exception inner) 
            : base(faultyInstruction, "The command source was not found. ", inner) { }
        internal CommandSourceNotFoundException(InvokeCommandInstruction faultyInstruction) 
            : this(faultyInstruction, null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        internal CommandSourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endif
    }
}