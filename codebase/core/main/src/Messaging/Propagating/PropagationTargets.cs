using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Axle.Extensions.Object;

namespace Forest.Messaging.Propagating
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public readonly struct PropagationTargets : IEquatable<PropagationTargets>
    {
        /// <summary>
        /// A <see cref="PropagationTargets"/> configuration that not trigger any subscriptions.
        /// </summary>
        public static readonly PropagationTargets None = new PropagationTargets(
            PropagationDirection.None, 
            PropagationRange.None);
        /// <summary>
        /// A <see cref="PropagationTargets"/> configuration that will subscribe only the sender node's parent.
        /// </summary>
        public static readonly PropagationTargets Parent = new PropagationTargets(
            PropagationDirection.Ancestors, 
            PropagationRange.Minimum);
        /// <summary>
        /// A <see cref="PropagationTargets"/> configuration that will subscribe only the sender node's direct
        /// descendants.
        /// </summary>
        public static readonly PropagationTargets Children = new PropagationTargets(
            PropagationDirection.Descendants, 
            PropagationRange.Minimum);
        /// <summary>
        /// A <see cref="PropagationTargets"/> configuration that will subscribe the sender node's parent and all of
        /// its ancestors.
        /// </summary>
        public static readonly PropagationTargets Ancestors = new PropagationTargets(
            PropagationDirection.Ancestors, 
            PropagationRange.Maximum);
        /// <summary>
        /// A <see cref="PropagationTargets"/> configuration that will subscribe the sender node's children and all of
        /// their descendents.
        /// </summary>
        public static readonly PropagationTargets Descendants = new PropagationTargets(
            PropagationDirection.Descendants, 
            PropagationRange.Maximum);
        /// <summary>
        /// A <see cref="PropagationTargets"/> configuration that will subscribe all of the sender node's siblings.
        /// </summary>
        public static readonly PropagationTargets Siblings = new PropagationTargets(
            PropagationDirection.Siblings, 
            PropagationRange.Minimum);
        /// <summary>
        /// A <see cref="PropagationTargets"/> configuration that will subscribe the sender node's descendants,
        /// the sender node's siblings and also the descendents of the sender node's siblings.
        /// </summary>
        public static readonly PropagationTargets SiblingsAndAllDescendants = new PropagationTargets(
            PropagationDirection.Siblings|PropagationDirection.Descendants, 
            PropagationRange.Maximum);
        /// <summary>
        /// A <see cref="PropagationTargets"/> configuration that will subscribe the sender node's direct descendants,
        /// and the sender node's siblings (but not their descendents).
        /// </summary>
        public static readonly PropagationTargets SiblingsAndOwnDescendants = new PropagationTargets(
            PropagationDirection.Siblings|PropagationDirection.Descendants, 
            PropagationRange.Minimum);

        private PropagationTargets(PropagationDirection direction, PropagationRange range)
        {
            Direction = direction;
            Range = range;
        }

        public bool Equals(PropagationTargets other) => Direction == other.Direction && Range == other.Range;
        public override bool Equals(object obj) => obj is PropagationTargets other && Equals(other);

        public override int GetHashCode() => this.CalculateHashCode(Direction, Range);

        public PropagationDirection Direction { get; }
        public PropagationRange Range { get; }
    }
}