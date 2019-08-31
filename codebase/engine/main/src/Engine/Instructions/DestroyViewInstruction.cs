using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [System.Serializable]
    #endif
    public sealed class DestroyViewInstruction : NodeStateModification
    {
        public DestroyViewInstruction(Tree.Node node) : base(node) { }

        protected override bool DoEquals(ForestInstruction other) => other is DestroyViewInstruction dv && Node.Equals(dv.Node);

        protected override int DoGetHashCode() => this.CalculateHashCode(Node);

        public void Deconstruct(out Tree.Node node) => node = Node;
    }
}