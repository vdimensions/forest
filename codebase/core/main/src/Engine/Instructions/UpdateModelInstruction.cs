using System;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class UpdateModelInstruction : TreeModification
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private readonly object _model;

        public UpdateModelInstruction(string nodeKey, object model) : base(nodeKey)
        {
            _model = model;
        }

        protected override bool IsEqualTo(TreeModification other)
        {
            return other is UpdateModelInstruction um && base.IsEqualTo(um) && Equals(Model, um.Model);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(NodeKey, Model);

        public object Model => _model;
    }
}