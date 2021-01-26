using System;
using System.Diagnostics;
using Axle.Extensions.Object;
using Axle.Verification;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Navigation
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class Location : IEquatable<Location>
    {
        public static readonly Location Empty = new Location();
        
        public static Location Create(string path, object state)
        {
            path.VerifyArgument(nameof(path)).IsNotNullOrEmpty();
            state.VerifyArgument(nameof(state)).IsNotNull();
            return new Location(path, state);
        }
        public static Location Create(string path)
        {
            path.VerifyArgument(nameof(path)).IsNotNullOrEmpty();
            return new Location(path);
        }
        [Obsolete("Use `Create` method overload instead")]
        public static Location FromPath(string path) => Create(path);

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _path;
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object _value;

        internal Location(string path, object value)
        {
            _path = path;
            _value = value;
        }
        internal Location(string path) : this(path, null) { }
        private Location() : this(string.Empty) { }

        bool IEquatable<Location>.Equals(Location other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            var cmp = StringComparer.Ordinal;
            return other != null 
                && cmp.Equals(Path, other.Path) 
                && Equals(Value, other.Value);
        }
        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Location ni && Equals(ni);

        /// <inheritdoc />
        public override int GetHashCode() => this.CalculateHashCode(StringComparer.Ordinal.GetHashCode(Path), Value);

        public string Path => _path ?? string.Empty;
        public object Value => _value;
    }
}