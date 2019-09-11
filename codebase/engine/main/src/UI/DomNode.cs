using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using Axle.Extensions.Object;

namespace Forest.UI
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
        private readonly int _index;

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
        private readonly ImmutableDictionary<string, ILinkModel> _links;

        public DomNode(string instanceId, int index, string name, string region, object model, DomNode parent, ImmutableDictionary<string, IEnumerable<DomNode>> regions, ImmutableDictionary<string, ICommandModel> commands, ImmutableDictionary<string, ILinkModel> links)
        {
            _instanceID = instanceId;
            _index = index;
            _name = name;
            _region = region;
            _model = model;
            _parent = parent;
            _regions = regions;
            _commands = commands;
            _links = links;
        }

        private bool DictionaryEquals<T>(IDictionary<string, T> left, IDictionary<string, T> right, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }
            var strComparer = StringComparer.Ordinal;
            if (left.Keys.Except(right.Keys, strComparer).Any())
            {
                return false;
            }
            if (right.Keys.Except(left.Keys, strComparer).Any())
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
                && _index == other._index
                && comparer.Equals(_name, other._name)
                && comparer.Equals(_region, other._region)
                && Equals(_model, other._model)
                && Equals(_parent, other._parent)
                && DictionaryEquals(_regions, other._regions, new DomNodesComparer())
                && DictionaryEquals(_commands, other._commands)
                && DictionaryEquals(_links, other._links);
        }

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is DomNode other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_instanceID != null ? _instanceID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _index;
                hashCode = (hashCode * 397) ^ (_name != null ? _name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_region != null ? _region.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_model != null ? _model.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_parent != null ? _parent.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_regions != null ? this.CalculateHashCode(_regions.ToArray()) : 0);
                hashCode = (hashCode * 397) ^ (_commands != null ? this.CalculateHashCode(_commands.ToArray()) : 0);
                hashCode = (hashCode * 397) ^ (_links != null ? this.CalculateHashCode(_links.ToArray()) : 0);
                return hashCode;
            }
        }

        public string InstanceID => _instanceID;
        public int Index => _index;
        public string Name => _name;
        public string Region => _region;
        public object Model => _model;
        public DomNode Parent => _parent;
        public ImmutableDictionary<string, IEnumerable<DomNode>> Regions => _regions;
        public ImmutableDictionary<string, ICommandModel> Commands => _commands;
        public ImmutableDictionary<string, ILinkModel> Links => _links;
    }
}