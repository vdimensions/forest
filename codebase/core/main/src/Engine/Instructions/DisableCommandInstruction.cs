using System;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class DisableCommandInstruction : CommandStateInstruction, IEquatable<DisableCommandInstruction>
    {
        public DisableCommandInstruction(string nodeKey, string command) : base(nodeKey, command) { }

        protected override bool IsEqualTo(CommandStateInstruction other) => other is DisableCommandInstruction && base.IsEqualTo(other);

        bool IEquatable<DisableCommandInstruction>.Equals(DisableCommandInstruction other) => other != null && base.IsEqualTo(other);
    }
}