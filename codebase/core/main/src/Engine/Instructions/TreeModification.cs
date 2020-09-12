using System;

namespace Forest.Engine.Instructions
{
    /// An abstract class to serve as a base for such instructions, whose effect
    /// must be distributed across other nodes of a forest cluster.
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public abstract class TreeModification : ForestInstruction
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.Runtime.Serialization.DataMember]
        #endif
        private string _nodeKey;

        internal TreeModification(string nodeKey) : base()
        {
            _nodeKey = nodeKey;
        }

        protected sealed override bool IsEqualTo(ForestInstruction other)
        {
            return other is TreeModification otherModification && IsEqualTo(otherModification);
        }

        protected virtual bool IsEqualTo(TreeModification treeModification)
        {
            return StringComparer.Ordinal.Equals(_nodeKey, treeModification.NodeKey);
        }

        public string NodeKey => _nodeKey;
    }
}