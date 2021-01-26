using System;
using System.Collections.Generic;
using System.Linq;
using Axle;
using Axle.Collections.Immutable;
using Axle.Extensions.Object;
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
        private sealed class DomNodesComparer : AbstractEqualityComparer<IReadOnlyCollection<DomNode>>
        {
            private readonly IEqualityComparer<DomNode> _comparer;

            public DomNodesComparer(IEqualityComparer<DomNode> comparer)
            {
                _comparer = comparer;
            }
            public DomNodesComparer() : this(EqualityComparer<DomNode>.Default) { }

            protected override bool DoEquals(IReadOnlyCollection<DomNode> x, IReadOnlyCollection<DomNode> y)
            {
                if (x.Count != y.Count)
                {
                    return false;
                }

                return x.SequenceEqual(y, _comparer);
            }

            protected override int DoGetHashCode(IReadOnlyCollection<DomNode> obj) => this.CalculateHashCode(obj.ToArray());
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
        private readonly ImmutableDictionary<string, IReadOnlyCollection<DomNode>> _regions;

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
                ImmutableDictionary<string, IReadOnlyCollection<DomNode>> regions, 
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

        private bool DictionaryKeysEquals(IEnumerable<string> left, IEnumerable<string> right, IEqualityComparer<string> comparer)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            return ImmutableHashSet.CreateRange(comparer, left).SymmetricExcept(right).Count == 0;
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
                && Equals(_model, other._model)
                && (ReferenceEquals(_parent, other._parent) || comparer.Equals(_parent._instanceID, other._parent._instanceID))
                && DictionaryKeysEquals(_regions.Keys, other._regions.Keys, comparer)
                && DictionaryKeysEquals(_commands.Keys, other._commands.Keys, comparer)
                && comparer.Equals(_resourceBundle, other._resourceBundle);
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
                hashCode = (hashCode * 397) ^ (_parent != null ? _parent.InstanceID.GetHashCode() : 0);
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
        public ImmutableDictionary<string, IReadOnlyCollection<DomNode>> Regions => _regions;
        public ImmutableDictionary<string, ICommandModel> Commands => _commands;
        public string ResourceBundle => _resourceBundle;
        public uint Revision => _revision;
    }
}