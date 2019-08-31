using System;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class EnableCommandInstruction : CommandStateInstruction, IEquatable<EnableCommandInstruction>
    {
        public EnableCommandInstruction(Tree.Node node, string command) : base(node, command) { }

        protected override bool DoEquals(ForestInstruction other) => other is EnableCommandInstruction && base.DoEquals(other);

        bool IEquatable<EnableCommandInstruction>.Equals(EnableCommandInstruction other) => other != null && base.DoEquals(other);
    }
}