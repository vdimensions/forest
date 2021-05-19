using System;
using System.Runtime.InteropServices;

namespace Forest.Messaging
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Letter : IComparable<Letter>, IEquatable<Letter>
    {
        public Letter(IView sender, object message, long timestamp, DistributionData distributionData)
        {
            Sender = sender;
            Message = message;
            Timestamp = timestamp;
            DistributionData = distributionData;
        }

        public int CompareTo(Letter other) => Timestamp.CompareTo(other.Timestamp);

        public override bool Equals(object obj) => obj is Letter other && Equals(other);

        public bool Equals(Letter other)
        {
            return ReferenceEquals(Sender, other.Sender) 
                   && Equals(Message, other.Message) 
                   && Equals(DistributionData, other.DistributionData) 
                   && Timestamp == other.Timestamp;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 848193896;
                hashCode = hashCode * -1521134295 + (Sender == null ? 0 : Sender.GetHashCode());
                hashCode = hashCode * -1521134295 + Message.GetHashCode();
                hashCode = hashCode * -1521134295 + DistributionData.GetHashCode();
                hashCode = hashCode * -1521134295 + Timestamp.GetHashCode();
                return hashCode;
            }
        }

        public IView Sender { get; }
        public object Message { get; }
        public DistributionData DistributionData { get; }
        private long Timestamp { get; }
    }
}