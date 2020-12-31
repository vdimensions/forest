using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [System.Serializable]
    #endif
    public sealed class DestroyViewInstruction : TreeModification
    {
        public DestroyViewInstruction(string nodeKey) : base(nodeKey) { }

        protected override bool IsEqualTo(TreeModification other) 
            => other is DestroyViewInstruction dv  && base.IsEqualTo(dv);

        protected override int DoGetHashCode() => this.CalculateHashCode(NodeKey);

    }
}