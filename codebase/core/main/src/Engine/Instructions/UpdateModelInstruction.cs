using System;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class UpdateModelInstruction : NodeStateModification
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private readonly object _model;

        public UpdateModelInstruction(Tree.Node node, object model) : base(node)
        {
            _model = model;
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            return other is UpdateModelInstruction um && Node.Equals(um.Node) && Equals(Model, um.Model);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(Node, Model);

        public void Deconstruct(out Tree.Node node, out object model)
        {
            node = Node;
            model = Model;
        }

        public object Model => _model;
    }
}