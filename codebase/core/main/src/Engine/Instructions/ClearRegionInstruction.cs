using System;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class ClearRegionInstruction : NodeStateModification
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private readonly string _region;

        public ClearRegionInstruction(Tree.Node node, string region) : base(node)
        {
            _region = region;
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            return other is ClearRegionInstruction sm
                && StringComparer.Ordinal.Equals(Region, sm.Region)
                && Node.Equals(sm.Node);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(Node, Region);

        public string Region => _region;
    }
}