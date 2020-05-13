using System;
using System.Diagnostics;
using System.Text;
using Forest.ComponentModel;

namespace Forest
{
    partial class Tree
    {
        /// <summary>
        /// A class representing a single node inside a Forest <see cref="Tree"/>.
        /// </summary>
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [Serializable]
        #endif
        [DebuggerDisplay("{this." + nameof(ToString) + "()}")]
        public sealed class Node : IComparable<Node>, IEquatable<Node>
        {
            public static Node Create(string region, ViewHandle viewHandle, Node parent)
            {
                return new Node(parent, region, viewHandle, GuidGenerator.NewID().ToString());
            }
            public static Node Create(string region, string viewName, Node parent)
            {
                return new Node(parent, region, ViewHandle.FromName(viewName), GuidGenerator.NewID().ToString());
            }
            public static Node Create(string region, Type viewType, Node parent)
            {
                return new Node(parent, region, ViewHandle.FromType(viewType), GuidGenerator.NewID().ToString());
            }

            public static readonly Node Shell = new Node(null, string.Empty, ViewHandle.FromName(string.Empty), Guid.Empty.ToString());

            internal Node(Node parent, string region, ViewHandle viewHandle, string instanceID) 
                : this(parent, region, viewHandle, instanceID, 0) { }
            private Node(Node parent, string region, ViewHandle viewHandle, string instanceID, int revision)
            {
                Parent = parent;
                Region = region;
                ViewHandle = viewHandle;
                InstanceID = instanceID;
                Revision = revision;
            }
            
            internal Node ChangeParent(Node newParent) => new Node(newParent, Region, ViewHandle, InstanceID, Revision);

            internal Node UpdateRevision() => new Node(Parent, Region, ViewHandle, InstanceID, Revision + 1);

            int IComparable<Node>.CompareTo(Node other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                return string.Compare(InstanceID, other.InstanceID, StringComparison.Ordinal);
            }

            /// <inheritdoc />
            public bool Equals(Node other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return StringComparer.Ordinal.Equals(InstanceID, other.InstanceID);
            }

            /// <inheritdoc />
            public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Node other && Equals(other);

            /// <inheritdoc />
            public override int GetHashCode() => (InstanceID != null ? InstanceID.GetHashCode() : 0);

            private StringBuilder ToStringBuilder(bool stopAtRegion)
            {
                var sb = Parent == null
                    ? new StringBuilder() 
                    : Parent.ToStringBuilder(false).Append(Region.Length == 0 ? "shell" : Region);
                return (stopAtRegion ? sb : sb.AppendFormat("/{0} #{1}", ViewHandle, InstanceID)).Append('/');
            }

            public override string ToString() => ToStringBuilder(false).ToString();

            public Node Parent { get; }
            /// <summary>
            /// Gets the name of the <see cref="IRegion"/> that contains the view represented by this <see cref="Node"/>.
            /// </summary>
            public string Region { get; }
            /// <summary>
            /// Gets a <see cref="ViewHandle"/> that can be used to obtain the <see cref="IViewDescriptor">view descriptor</see>
            /// </summary>
            public ViewHandle ViewHandle { get; }
            /// <summary>
            /// A unique string identifying the current node.
            /// </summary>
            public string InstanceID { get; }
            /// <summary>
            /// Gets a number indicating the current node's revision.
            /// The revision number changes with each update of a node.
            /// </summary>
            public int Revision { get; } = 0;

            //public string RegionSegment => ToStringBuilder(true).ToString();
        }
    }
}