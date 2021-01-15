using System.Collections;
using System.Collections.Generic;

namespace Forest.Collections.Immutable
{
    public static class ImmutableHashSet
    {
        public static ImmutableHashSet<T> Create<T>()
            => new ImmutableHashSet<T>(System.Collections.Immutable.ImmutableHashSet.Create<T>());
        public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> comparer)
            => new ImmutableHashSet<T>(System.Collections.Immutable.ImmutableHashSet.Create<T>(comparer));
        
        public static ImmutableHashSet<T> CreateRange<T>(IEqualityComparer<T> comparer, IEnumerable<T> items)
            => new ImmutableHashSet<T>(System.Collections.Immutable.ImmutableHashSet.CreateRange(comparer, items));
    }

    public class ImmutableHashSet<T> : IReadOnlyCollection<T>
    {
        public static readonly ImmutableHashSet<T> Empty = ImmutableHashSet.Create<T>();
        
        private readonly System.Collections.Immutable.ImmutableHashSet<T> _impl;

        internal ImmutableHashSet(System.Collections.Immutable.ImmutableHashSet<T> impl)
        {
            _impl = impl;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => _impl.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => _impl.Count;

        /// <summary>Retrieves an empty immutable set that has the same sorting and ordering semantics as this instance.</summary>
        /// <returns>An empty set that has the same sorting and ordering semantics as this instance.</returns>
        public ImmutableHashSet<T> Clear() => ImmutableHashSet.CreateRange(Comparer, _impl.Clear());

        /// <summary>Determines whether this immutable set contains a specified element.</summary>
        /// <param name="value">The element to locate in the set.</param>
        /// <returns>
        /// <see langword="true" /> if the set contains the specified value; otherwise, <see langword="false" />.</returns>
        public bool Contains(T value) => _impl.Contains(value);

        /// <summary>Adds the specified element to this immutable set.</summary>
        /// <param name="value">The element to add.</param>
        /// <returns>A new set with the element added, or this set if the element is already in the set.</returns>
        public ImmutableHashSet<T> Add(T value)
        {
            return ImmutableHashSet.CreateRange<T>(Comparer, _impl.Add(value));
        }

        /// <summary>Removes the specified element from this immutable set.</summary>
        /// <param name="value">The element to remove.</param>
        /// <returns>A new set with the specified element removed, or the current set if the element cannot be found in the set.</returns>
        public ImmutableHashSet<T> Remove(T value)
        {
            return ImmutableHashSet.CreateRange(Comparer, _impl.Remove(value));
        }

        /// <summary>Determines whether the set contains a specified value.</summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">The matching value from the set, if found, or <c>equalvalue</c> if there are no matches.</param>
        /// <returns>
        /// <see langword="true" /> if a matching value was found; otherwise, <see langword="false" />.</returns>
        public bool TryGetValue(T equalValue, out T actualValue) => _impl.TryGetValue(equalValue, out actualValue);

        /// <summary>Creates an immutable set that contains only elements that exist in this set and the specified set.</summary>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Immutable.IImmutableSet`1" />.</param>
        /// <returns>A new immutable set that contains elements that exist in both sets.</returns>
        public ImmutableHashSet<T> Intersect(IEnumerable<T> other)
        {
            return ImmutableHashSet.CreateRange(Comparer, _impl.Intersect(other));
        }

        /// <summary>Removes the elements in the specified collection from the current immutable set.</summary>
        /// <param name="other">The collection of items to remove from this set.</param>
        /// <returns>A new set with the items removed; or the original set if none of the items were in the set.</returns>
        public ImmutableHashSet<T> Except(IEnumerable<T> other)
        {
            return ImmutableHashSet.CreateRange<T>(Comparer, _impl.Except(other));
        }

        /// <summary>Creates an immutable set that contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set that contains the elements that are present only in the current set or in the specified collection, but not both.</returns>
        public ImmutableHashSet<T> SymmetricExcept(IEnumerable<T> other)
        {
            return ImmutableHashSet.CreateRange<T>(Comparer, _impl.SymmetricExcept(other));
        }

        /// <summary>Creates a new immutable set that contains all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new immutable set with the items added; or the original set if all the items were already in the set.</returns>
        public ImmutableHashSet<T> Union(IEnumerable<T> other)
        {
            return ImmutableHashSet.CreateRange<T>(Comparer, _impl.Union(other));
        }

        /// <summary>Determines whether the current immutable set and the specified collection contain the same elements.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <see langword="true" /> if the sets are equal; otherwise, <see langword="false" />.</returns>
        public bool SetEquals(IEnumerable<T> other) => _impl.SetEquals(other);

        /// <summary>Determines whether the current immutable set is a proper (strict) subset of the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <see langword="true" /> if the current set is a proper subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other) => _impl.IsProperSubsetOf(other);

        /// <summary>Determines whether the current immutable set is a proper (strict) superset of the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <see langword="true" /> if the current set is a proper superset of the specified collection; otherwise, false.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other) => _impl.IsProperSupersetOf(other);

        /// <summary>Determines whether the current immutable set is a subset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <see langword="true" /> if the current set is a subset of the specified collection; otherwise, <see langword="false" />.
        /// </returns>
        public bool IsSubsetOf(IEnumerable<T> other) => _impl.IsSubsetOf(other);

        /// <summary>
        /// Determines whether the current immutable hash set is a superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param></param>
        /// <returns>
        /// <see langword="true" /> if the current set is a superset of the specified collection; otherwise, <see langword="false" />.
        /// </returns>
        public bool IsSupersetOf(IEnumerable<T> other) => _impl.IsSupersetOf(other);

        /// <summary>Determines whether the current immutable set overlaps with the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <see langword="true" /> if the current set and the specified collection share at least one common element; otherwise, <see langword="false" />.</returns>
        public bool Overlaps(IEnumerable<T> other) => _impl.Overlaps(other);

        /// <summary>
        /// Gets the object that is used to obtain hash codes for the keys and to check the equality of values in the immutable hash set.
        /// </summary>
        public IEqualityComparer<T> Comparer => _impl.KeyComparer;
    }
}