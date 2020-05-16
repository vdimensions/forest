using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Forest.ComponentModel;

namespace Forest
{
    partial class Tree
    {
        /// <summary>
        /// A struct representing a single node inside a Forest <see cref="Tree"/>.
        /// </summary>
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [Serializable]
        #endif
        [StructLayout(LayoutKind.Sequential)]
        [DebuggerDisplay("{this." + nameof(ToString) + "()}")]
        public struct Node : IComparable<Node>, IEquatable<Node>
        {
            internal static Node Create(string key, ViewHandle viewHandle, string region, object model, Node parent)
            {
                var viewState = model == null ? null : new ViewState?(Forest.ViewState.Create(model));
                
                return new Node(parent.Key, region, viewHandle, key, viewState, 0u);
            }

            public static readonly Node Shell;
            
            private readonly string _region;
            private readonly ViewHandle _viewHandle;
            private readonly string _key;
            private readonly string _parentKey;

            internal Node(Node parent, string region, ViewHandle viewHandle, string key) 
                : this(parent.Key, region, viewHandle, key, null, 0u) { }
            private Node(string parentKey, string region, ViewHandle viewHandle, string key, ViewState? viewState, uint revision)
            {
                _key = key;
                _parentKey = parentKey;
                _region = region;
                _viewHandle = viewHandle;
                ViewState = viewState;
                Revision = revision;
            }
            
            int IComparable<Node>.CompareTo(Node other)
            {
                return StringComparer.Ordinal.Compare(Key, other.Key);
            }

            /// <inheritdoc />
            public bool Equals(Node other)
            {
                return StringComparer.Ordinal.Equals(Key, other.Key);
            }

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is Node other && Equals(other);

            /// <inheritdoc />
            public override int GetHashCode() => (Key != null ? Key.GetHashCode() : 0);

            internal Node UpdateRevision() => new Node(ParentKey, Region, ViewHandle, Key, ViewState, 1u + Revision);

            internal Node SetViewState(ViewState viewState)
            {
                return new Node(ParentKey, Region, ViewHandle, Key, viewState, 1u + Revision);
            }

            // private StringBuilder ToStringBuilder(bool stopAtRegion)
            // {
            //     var sb = Parent == null
            //         ? new StringBuilder() 
            //         : Parent.ToStringBuilder(false).Append(Region.Length == 0 ? "shell" : Region);
            //     return (stopAtRegion ? sb : sb.AppendFormat("/{0} #{1}", ViewHandle, Key)).Append('/');
            // }

            //public override string ToString() => ToStringBuilder(false).ToString();

            public string ParentKey => _parentKey ?? string.Empty;

            /// <summary>
            /// Gets the name of the <see cref="IRegion"/> that contains the view represented by this <see cref="Node"/>.
            /// </summary>
            public string Region => _region ?? string.Empty;

            /// <summary>
            /// Gets a <see cref="ViewHandle"/> that can be used to obtain the <see cref="IViewDescriptor">view descriptor</see>
            /// </summary>
            public ViewHandle ViewHandle => _viewHandle ?? ViewHandle.FromName(string.Empty);

            /// <summary>
            /// A unique string identifying the current node.
            /// </summary>
            public string Key => _key ?? Guid.Empty.ToString();
            
            public ViewState? ViewState { get; }
            public uint Revision { get; }

            //public string RegionSegment => ToStringBuilder(true).ToString();
        }
    }
}