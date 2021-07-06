using System;
using Forest.Engine.Instructions;

namespace Forest.Commands.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class EnableCommandInstruction : CommandStateInstruction, IEquatable<EnableCommandInstruction>
    {
        public EnableCommandInstruction(string nodeKey, string command) : base(nodeKey, command) { }

        protected override bool IsEqualTo(CommandStateInstruction other) => other is EnableCommandInstruction && base.IsEqualTo(other);

        bool IEquatable<EnableCommandInstruction>.Equals(EnableCommandInstruction other) => other != null && base.IsEqualTo(other);
    }
}