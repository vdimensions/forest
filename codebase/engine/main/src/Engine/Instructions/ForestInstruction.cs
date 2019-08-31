using System;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public abstract class ForestInstruction : IEquatable<ForestInstruction>
    {
        /// An internal default constructor to prevent creating custom instances of this class.
        internal ForestInstruction() {}

        protected abstract bool DoEquals(ForestInstruction other);

        protected abstract int DoGetHashCode();

        public sealed override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals((ForestInstruction) obj);
        }
        bool IEquatable<ForestInstruction>.Equals(ForestInstruction other) => DoEquals(other);

        public sealed override int GetHashCode() => DoGetHashCode();
    }
}