using System;
using Axle.Extensions.Object;
using Axle.Verification;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public abstract class CommandStateInstruction : NodeStateModification
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private readonly string _command;

        internal CommandStateInstruction(Tree.Node node, string command) : base(node)
        {
            _command = command.VerifyArgument(nameof(command)).IsNotNullOrEmpty();
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            return other is CommandStateInstruction csi && other.GetType() == GetType() && Node.Equals(csi.Node) && StringComparer.Ordinal.Equals(Command, csi.Command);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(GetType(), Node, Command);

        public void Deconstruct(out Tree.Node node, out string command)
        {
            node = Node;
            command = Command;
        }

        public string Command => _command;
    }
}