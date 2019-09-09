using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
                (x == null && y == null) || (x != null && y != null && x.SequenceEqual(y, _comparer));

            public int GetHashCode(IEnumerable<DomNode> obj) => obj == null ? 0 : this.CalculateHashCode(obj.ToArray());
        }

        public static bool operator ==(DomNode left, DomNode right) => Equals(left, right);

        public static bool operator !=(DomNode left, DomNode right) => !Equals(left, right);

        public DomNode(string instanceId, int index, string name, string region, object model, DomNode parent, IImmutableDictionary<string, IEnumerable<DomNode>> regions, IImmutableDictionary<string, ICommandModel> commands, IImmutableDictionary<string, ILinkModel> links)
        {
            InstanceID = instanceId;
            Index = index;
            Name = name;
            Region = region;
            Model = model;
            Parent = parent;
            Regions = regions;
            Commands = commands;
            Links = links;
        }

        private bool DictionaryEquals<T>(IImmutableDictionary<string, T> left, IImmutableDictionary<string, T> right, IEqualityComparer<T> comparer = null)
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
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var comparer = StringComparer.Ordinal;
            return comparer.Equals(InstanceID, other.InstanceID) 
                && Index == other.Index 
                && comparer.Equals(Name, other.Name) 
                && comparer.Equals(Region, other.Region)
                && Equals(Model, other.Model) 
                && Equals(Parent, other.Parent) 
                && DictionaryEquals(Regions, other.Regions, new DomNodesComparer()) 
                && DictionaryEquals(Commands, other.Commands) 
                && DictionaryEquals(Links, other.Links);
        }

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is DomNode other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (InstanceID != null ? InstanceID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Index;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Region != null ? Region.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Model != null ? Model.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Parent != null ? Parent.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Regions != null ? this.CalculateHashCode(Regions.ToArray()) : 0);
                hashCode = (hashCode * 397) ^ (Commands != null ? this.CalculateHashCode(Commands.ToArray()) : 0);
                hashCode = (hashCode * 397) ^ (Links != null ? this.CalculateHashCode(Links.ToArray()) : 0);
                return hashCode;
            }
        }

        public string InstanceID { get; }
        public int Index { get; }
        public string Name { get; }
        public string Region { get; }
        public object Model { get; }
        public DomNode Parent { get; }
        public IImmutableDictionary<string, IEnumerable<DomNode>> Regions { get; }
        public IImmutableDictionary<string, ICommandModel> Commands { get; }
        public IImmutableDictionary<string, ILinkModel> Links { get; }
    }
}