using System;
using Axle.Verification;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class NodeNotExpectedException : ForestInstructionException
    {
        public NodeNotExpectedException(ForestInstruction instruction, Tree.Node node) : this(instruction, node, null) { }
        public NodeNotExpectedException(ForestInstruction instruction, Tree.Node node, Exception inner) 
            : base(instruction, string.Format("Unable to process instruction {0} - node '{1}' was not expected to exist. ", instruction.VerifyArgument(nameof(instruction)).IsNotNull().Value, node), inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected NodeNotExpectedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}