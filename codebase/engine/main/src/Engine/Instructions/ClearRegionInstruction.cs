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
        private string _region;

        public ClearRegionInstruction(Tree.Node node, string region) : base(node)
        {
            _region = region;
        }

        protected override bool DoEquals(ForestInstruction other)
        {
            var cmp = StringComparer.Ordinal;
            return other is ClearRegionInstruction sm
                   && cmp.Equals(Region, sm.Region)
                   && Node.Equals(sm.Node);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(Node, Region);

        public void Deconstruct(out Tree.Node node, out string region)
        {
            node = Node;
            region = Region;
        }

        public string Region => _region;
    }
}