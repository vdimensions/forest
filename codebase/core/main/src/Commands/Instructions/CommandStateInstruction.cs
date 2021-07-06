using System;
using Axle.Extensions.Object;
using Axle.Verification;
using Forest.Engine.Instructions;

namespace Forest.Commands.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public abstract class CommandStateInstruction : TreeModification
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private readonly string _command;

        internal CommandStateInstruction(string nodeKey, string command) : base(nodeKey)
        {
            _command = command.VerifyArgument(nameof(command)).IsNotNullOrEmpty();
        }

        protected sealed override bool IsEqualTo(TreeModification other)
        {
            return other is CommandStateInstruction csi
                && base.IsEqualTo(other)  
                && IsEqualTo(csi);
        }
        protected virtual bool IsEqualTo(CommandStateInstruction other)
        {
            return other.GetType() == GetType() 
                && StringComparer.Ordinal.Equals(Command, other.Command);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(GetType(), NodeKey, Command);

        public string Command => _command;
    }
}