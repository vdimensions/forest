using System;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class DisableCommandInstruction : CommandStateInstruction, IEquatable<DisableCommandInstruction>
    {
        public DisableCommandInstruction(Tree.Node node, string command) : base(node, command) { }

        protected override bool DoEquals(ForestInstruction other) => other is DisableCommandInstruction && base.DoEquals(other);

        bool IEquatable<DisableCommandInstruction>.Equals(DisableCommandInstruction other) => other != null && base.DoEquals(other);
    }
}