using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Axle.Verification;
using Forest.ComponentModel;

namespace Forest
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [SuppressMessage("ReSharper", "EmptyConstructor")]
    [DebuggerDisplay("{this." + nameof(ToString) + "()}")]
    [StructLayout(LayoutKind.Sequential)]
    public abstract class ViewHandle : IEquatable<ViewHandle>
    {
        public static ViewHandle FromType(Type type) => new TypedViewHandle(type.VerifyArgument(nameof(type)).IsNotNull().Is<IView>().Value);
        public static ViewHandle FromName(string name) => new NamedViewHandle(name.VerifyArgument(nameof(name)).IsNotNull().Value);

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [Serializable]
        #endif
        [StructLayout(LayoutKind.Sequential)]
        internal sealed class TypedViewHandle : ViewHandle, IEquatable<TypedViewHandle>
        {
            public TypedViewHandle(Type viewType)
            {
                ViewType = viewType;
            }

            public override bool Equals(ViewHandle other) => other is TypedViewHandle tvh && Equals(tvh);
            public bool Equals(TypedViewHandle other)
            {
                return !ReferenceEquals(null, other) && 
                       (ReferenceEquals(this, other) || ViewType == other.ViewType);
            }

            public override int GetHashCode() => (ViewType != null ? ViewType.GetHashCode() : 0);

            public override string ToString() =>
                new StringBuilder("View by type `")
                    .Append(ForestViewDescriptor.GetAnonymousViewName(ViewType))
                    .Append("`")
                    .ToString();

            public Type ViewType { get; }
        }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [Serializable]
        #endif
        [StructLayout(LayoutKind.Sequential)]
        internal sealed class NamedViewHandle : ViewHandle, IEquatable<NamedViewHandle>
        {
            public NamedViewHandle(string name)
            {
                Name = name;
            }

            public override bool Equals(ViewHandle other) => other is NamedViewHandle nvh && Equals(nvh);
            public bool Equals(NamedViewHandle other)
            {
                return !ReferenceEquals(null, other) &&
                       (ReferenceEquals(this, other) || StringComparer.Ordinal.Equals(Name, other.Name));
            }

            public override int GetHashCode() => (Name != null ? Name.GetHashCode() : 0);

            public override string ToString() => new StringBuilder("View by name '").Append(Name).Append("'").ToString();

            public string Name { get; }
        }

        internal ViewHandle() { }

        public abstract bool Equals(ViewHandle other);
        public sealed override bool Equals(object obj) => obj is ViewHandle handle && Equals(handle);
    }
}
