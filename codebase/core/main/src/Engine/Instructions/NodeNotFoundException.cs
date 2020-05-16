using System;
using Axle.Verification;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class NodeNotFoundException : ForestInstructionException
    {
        internal NodeNotFoundException(ForestInstruction instruction, string nodeKey) : this(instruction, nodeKey, null) { }
        internal NodeNotFoundException(ForestInstruction instruction, string nodeKey, Exception inner) 
            : base(
                instruction, 
                string.Format(
                    "Unable to process instruction {0} - node '{1}' cannot be located. ", 
                    instruction.VerifyArgument(nameof(instruction)).IsNotNull().Value, 
                    nodeKey.VerifyArgument(nameof(nodeKey)).IsNotNull().Value), 
                inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        internal NodeNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}