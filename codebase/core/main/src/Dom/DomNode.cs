using System;
using System.Collections.Generic;
using System.Linq;
using Axle.Extensions.Object;
using Forest.Collections;
using Forest.Collections.Immutable;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Dom
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class DomNode : IEquatable<DomNode>
    {
        private sealed class DomNodesComparer : IEqualityComparer<IEnumerable<DomNode>>
        {
            private readonly IEqualityComparer<DomNode> _comparer;

            public DomNodesComparer(IEqualityComparer<DomNode> comparer)
            {
                _comparer = comparer;
            }
            public DomNodesComparer() : this(EqualityComparer<DomNode>.Default) { }

            public bool Equals(IEnumerable<DomNode> x, IEnumerable<DomNode> y) =>
                (x is null && y is null) || (x != null && y != null && x.SequenceEqual(y, _comparer));

            public int GetHashCode(IEnumerable<DomNode> obj) => obj == null ? 0 : this.CalculateHashCode(obj.ToArray());
        }

        /// Determines whether the specified <see cref="DomNode"/> instances are considered equal.
        public static bool operator == (DomNode left, DomNode right) => Equals(left, right);

        /// Determines whether the specified <see cref="DomNode"/> instances are considered not equal.
        public static bool operator != (DomNode left, DomNode right) => !Equals(left, right);

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly string _instanceID;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly string _name;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly string _region;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly object _model;
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly DomNode _parent;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly ImmutableDictionary<string, IEnumerable<DomNode>> _regions;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly ImmutableDictionary<string, ICommandModel> _commands;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly uint _revision;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly string _resourceBundle;

        internal DomNode(
                string instanceId, 
                string name, 
                string region, 
                object model, 
                DomNode parent, 
                ImmutableDictionary<string, IEnumerable<DomNode>> regions, 
                ImmutableDictionary<string, ICommandModel> commands,
                string resourceBundle,
                uint revision)
        {
            _instanceID = instanceId;
            _name = name;
            _region = region;
            _model = model;
            _parent = parent;
            _regions = regions;
            _commands = commands;
            _revision = revision;
            _resourceBundle = resourceBundle;
        }
        
        private static bool ExceptAny(IEnumerable<string> a, IEnumerable<string> b, IEqualityComparer<string> comparer)
        {
            var set1 = new HashSet<string>(a, comparer);
            var set2 = new HashSet<string>(b, comparer);
            set2.ExceptWith(set1);
            return set2.Count == 0;
        }

        private bool DictionaryEquals<T>(IReadOnlyDictionary<string, T> left, IReadOnlyDictionary<string, T> right, IEqualityComparer<T> comparer)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if (left.Count != right.Count)
            {
                return false;
            }
            if (left.Count == 0)
            {
                return true;
            }
            
            var strComparer = StringComparer.Ordinal;
            
            if (ExceptAny(left.Keys, right.Keys, strComparer))
            {
                return false;
            }
            if (ExceptAny(right.Keys, left.Keys, strComparer))
            {
                return false;
            }
            return left.Keys.All(key => comparer.Equals(left[key], right[key]));
        }

        public bool Equals(DomNode other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            
            var comparer = StringComparer.Ordinal;
            return comparer.Equals(_instanceID, other._instanceID)
                && comparer.Equals(_name, other._name)
                && comparer.Equals(_region, other._region)
                && comparer.Equals(_resourceBundle, other._resourceBundle)
                && DictionaryEquals(_regions, other._regions, new DomNodesComparer())
                && DictionaryEquals(_commands, other._commands, new CommandModelEqualityComparer())
                && Equals(_model, other._model)
                && Equals(_parent, other._parent);
        }

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is DomNode other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _instanceID != null ? _instanceID.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (_name != null ? _name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_region != null ? _region.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_model != null ? _model.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_parent != null ? _parent.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_regions != null ? this.CalculateHashCode(_regions.Cast<object>().ToArray()) : 0);
                hashCode = (hashCode * 397) ^ (_commands != null ? this.CalculateHashCode(_commands.Cast<object>().ToArray()) : 0);
                hashCode = (hashCode * 397) ^ (_resourceBundle != null ? this.CalculateHashCode(_resourceBundle.Cast<object>().ToArray()) : 0);
                return hashCode;
            }
        }

        public string InstanceID => _instanceID;
        public string Name => _name;
        public string Region => _region;
        public object Model => _model;
        public DomNode Parent => _parent;
        public ImmutableDictionary<string, IEnumerable<DomNode>> Regions => _regions;
        public ImmutableDictionary<string, ICommandModel> Commands => _commands;
        public string ResourceBundle => _resourceBundle;
        public uint Revision => _revision;
    }
}