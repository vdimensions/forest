using System;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class InstantiateViewInstruction : NodeStateModification
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private object _model;

        public InstantiateViewInstruction(Tree.Node node, object model) : base(node)
        {
            _model = model;
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            return other is InstantiateViewInstruction ivi && Node.Equals(ivi.Node) && Equals(Model, ivi.Model);
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