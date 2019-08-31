using System;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Engine.Instructions
{
    /// <summary>
    /// An exception that occurs when a command is being invoked.
    /// </summary>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class CommandSourceNotFoundException : CommandInstructionException
    {
        public CommandSourceNotFoundException(InvokeCommandInstruction faultyInstruction, Exception inner) 
            : base(faultyInstruction, "The command source was not found. ", inner) { }
        public CommandSourceNotFoundException(InvokeCommandInstruction faultyInstruction) 
            : this(faultyInstruction, null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected CommandSourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endif
    }
}