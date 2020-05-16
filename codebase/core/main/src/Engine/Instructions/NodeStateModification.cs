using System;

namespace Forest.Engine.Instructions
{
    /// An abstract class to serve as a base for such instructions, whose effect
    /// must be distributed across other nodes of a forest cluster.
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public abstract class NodeStateModification : ForestInstruction
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private string _nodeKey;

        internal NodeStateModification(string nodeKey) : base()
        {
            _nodeKey = nodeKey;
        }

        protected sealed override bool IsEqualTo(ForestInstruction other)
        {
            return other is NodeStateModification otherModification && IsEqualTo(otherModification);
        }

        protected virtual bool IsEqualTo(NodeStateModification nodeStateModification)
        {
            return StringComparer.Ordinal.Equals(_nodeKey, nodeStateModification.NodeKey);
        }

        public string NodeKey => _nodeKey;
    }
}