using System;
using Axle.Extensions.Object;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Navigation
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class NavigationInfo : IEquatable<NavigationInfo>
    {
        public static readonly NavigationInfo Empty = new NavigationInfo();
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly string _path;
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly object _state;

        internal NavigationInfo(string path, object state)
        {
            _path = path;
            _state = state;
        }
        internal NavigationInfo(string path) : this(path, null) { }
        private NavigationInfo() : this(string.Empty) { }

        bool IEquatable<NavigationInfo>.Equals(NavigationInfo other)
        {
            var cmp = StringComparer.Ordinal;
            return other != null && cmp.Equals(Path, other.Path) && Equals(State, other.State);
        }
        /// <inheritdoc />
        public override bool Equals(object obj) => obj is NavigationInfo ni && Equals(ni);

        /// <inheritdoc />
        public override int GetHashCode() => this.CalculateHashCode(StringComparer.Ordinal.GetHashCode(Path), State);

        public string Path => _path;
        public object State => _state;
    }
}