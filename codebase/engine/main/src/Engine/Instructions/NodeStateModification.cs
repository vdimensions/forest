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
        private Tree.Node _node;

        internal NodeStateModification(Tree.Node node) : base()
        {
            _node = node;
        }

        public Tree.Node Node => _node;
    }
}