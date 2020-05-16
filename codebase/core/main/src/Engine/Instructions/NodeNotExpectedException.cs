using System;
using Axle.Verification;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class NodeNotExpectedException : ForestInstructionException
    {
        internal NodeNotExpectedException(ForestInstruction instruction, string nodeKey) : this(instruction, nodeKey, null) { }
        internal NodeNotExpectedException(ForestInstruction instruction, string nodeKey, Exception inner) 
            : base(instruction, string.Format("Unable to process instruction {0} - node '{1}' was not expected to exist. ", instruction.VerifyArgument(nameof(instruction)).IsNotNull().Value, nodeKey), inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        internal NodeNotExpectedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}