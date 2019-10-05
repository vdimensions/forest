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
    public sealed class CommandNotFoundException : CommandInstructionException
    {
        public CommandNotFoundException(InvokeCommandInstruction faultyInstruction, Exception inner) 
            : base(faultyInstruction, "Command not found. ", inner) { }
        public CommandNotFoundException(InvokeCommandInstruction faultyInstruction) 
            : this(faultyInstruction, null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected CommandNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endif
    }
}