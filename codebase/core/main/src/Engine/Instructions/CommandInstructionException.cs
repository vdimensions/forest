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
    public class CommandInstructionException : ForestInstructionException
    {
        protected CommandInstructionException(InvokeCommandInstruction faultyInstruction, string message, Exception inner) 
            : base(faultyInstruction, string.Format("An error occurred while invoking command '{0}'. {1}", faultyInstruction.CommandName, message), inner) { }
        internal CommandInstructionException(InvokeCommandInstruction faultyInstruction, Exception inner) 
            : this(faultyInstruction, string.Empty, inner) { }
        internal CommandInstructionException(InvokeCommandInstruction faultyInstruction) 
            : this(faultyInstruction, null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        internal CommandInstructionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endif
    }
}