﻿using System;
using System.Diagnostics;
using Axle.Extensions.Object;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Navigation
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class NavigationState : IEquatable<NavigationState>
    {
        public static readonly NavigationState Empty = new NavigationState();
        
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

        internal NavigationState(string path, object value)
        {
            _path = path;
            _value = value;
        }
        internal NavigationState(string path) : this(path, null) { }
        private NavigationState() : this(string.Empty) { }

        bool IEquatable<NavigationState>.Equals(NavigationState other)
        {
            var cmp = StringComparer.Ordinal;
            return other != null && cmp.Equals(Path, other.Path) && Equals(Value, other.Value);
        }
        /// <inheritdoc />
        public override bool Equals(object obj) => obj is NavigationState ni && Equals(ni);

        /// <inheritdoc />
        public override int GetHashCode() => this.CalculateHashCode(StringComparer.Ordinal.GetHashCode(Path), Value);

        public string Path => _path ?? string.Empty;
        public object Value => _value;
    }
}