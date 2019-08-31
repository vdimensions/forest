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
        private string _command;

        internal CommandStateInstruction(Tree.Node node, string command) : base(node)
        {
            _command = command.VerifyArgument(nameof(command)).IsNotNullOrEmpty();
        }

        protected override bool DoEquals(ForestInstruction other)
        {
            return other is CommandStateInstruction um && Node.Equals(um.Node) && StringComparer.Ordinal.Equals(Command, um.Command);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(Node, Command);

        public void Deconstruct(out Tree.Node node, out string command)
        {
            node = Node;
            command = Command;
        }

        public string Command => _command;
    }
}