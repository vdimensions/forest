using System;
using Axle.Verification;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class InstructionNotSupportedException : ForestInstructionException
    {
        internal InstructionNotSupportedException(ForestInstruction faultyInstruction) 
            : base(faultyInstruction, string.Format("Unsupported instruction type `{0}`. ", faultyInstruction.VerifyArgument(nameof(faultyInstruction)).IsNotNull().Value.GetType().AssemblyQualifiedName), null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        internal InstructionNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}