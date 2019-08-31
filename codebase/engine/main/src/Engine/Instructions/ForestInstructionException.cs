using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Verification;

namespace Forest.Engine.Instructions
{
    /// <summary>
    /// An exception that is thrown when a forest instruction fails. 
    /// </summary>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public class ForestInstructionException : ForestException
    {
        private readonly ForestInstruction _faultyInstruction;
        private readonly Type _faultyInstructionType;

        protected ForestInstructionException(ForestInstruction faultyInstruction, string message, Exception inner)
            : base(string.Format("{0}{1}", message ?? string.Empty, inner == null ? string.Empty : "See inner exception for more details. "), inner)
        {
            _faultyInstruction = faultyInstruction.VerifyArgument(nameof(faultyInstruction)).IsNotNull();
        }
        public ForestInstructionException(ForestInstruction faultyInstruction, Exception inner) 
            : this(faultyInstruction, "Failed processing instruction. ", inner)
        {
            _faultyInstruction = faultyInstruction.VerifyArgument(nameof(faultyInstruction)).IsNotNull();
        }
        public ForestInstructionException(ForestInstruction faultyInstruction) : this(faultyInstruction, null) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected ForestInstructionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            _faultyInstructionType = (Type) info.GetValue(nameof(_faultyInstructionType), typeof(Type));
            _faultyInstruction = (ForestInstruction) info.GetValue(nameof(_faultyInstruction), _faultyInstructionType);
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(_faultyInstructionType), _faultyInstruction.GetType());
            info.AddValue(nameof(_faultyInstruction), _faultyInstruction);
        }
        #endif

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public ForestInstruction FaultyInstruction => _faultyInstruction;
    }
}
