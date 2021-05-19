using System;
using System.Linq;
using System.Runtime.InteropServices;
using Axle.Extensions.Object;
using Forest.Messaging.Propagating;

namespace Forest.Messaging
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct DistributionData : IEquatable<DistributionData>
    {
        public DistributionData(params string[] topics) : this()
        {
            PropagationTargets = PropagationTargets.None;
            Topics = topics;
        }
        public DistributionData(PropagationTargets propagationTargets) : this()
        {
            PropagationTargets = propagationTargets;
            Topics = null;
        }

        public bool Equals(DistributionData other)
        {
            return Equals(PropagationTargets, other.PropagationTargets) 
                && Topics.SequenceEqual(other.Topics);
        }
        public override bool Equals(object obj) => obj is DistributionData other && Equals(other);

        public override int GetHashCode()
        {
            return this.CalculateHashCode(PropagationTargets, this.CalculateHashCode(Topics));
        }
        
        public PropagationTargets PropagationTargets { get; }
        public string[] Topics { get; }
    }
}