using System;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class ClearRegionInstruction : TreeModification
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private readonly string _region;

        public ClearRegionInstruction(string nodeKey, string region) : base(nodeKey)
        {
            _region = region;
        }

        protected override bool IsEqualTo(TreeModification other)
        {
            return other is ClearRegionInstruction sm
                && StringComparer.Ordinal.Equals(Region, sm.Region)
                && base.IsEqualTo(other);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(NodeKey, Region);

        public string Region => _region;
    }
}