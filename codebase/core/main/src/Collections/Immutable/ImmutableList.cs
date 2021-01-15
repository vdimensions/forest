using System;
using System.Collections;
using System.Collections.Generic;

namespace Forest.Collections.Immutable
{
    public static class ImmutableList
    {
        public static ImmutableList<T> Create<T>()
            => new ImmutableList<T>(System.Collections.Immutable.ImmutableList.Create<T>());
        
        public static ImmutableList<T> CreateRange<T>(IEnumerable<T> items)
            => new ImmutableList<T>(System.Collections.Immutable.ImmutableList.CreateRange<T>(items));
    }
    
    public class ImmutableList<T> : IReadOnlyCollection<T>
    {
        public static readonly ImmutableList<T> Empty = ImmutableList.Create<T>();
        
        private readonly System.Collections.Immutable.ImmutableList<T> _impl;

        internal ImmutableList(System.Collections.Immutable.ImmutableList<T> impl)
        {
            _impl = impl;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => _impl.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _impl).GetEnumerator();

        public ImmutableList<T> Clear() => ImmutableList.CreateRange(_impl.Clear());

        public int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) 
            => _impl.IndexOf(item, index, count, equalityComparer);

        public int LastIndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) 
            => _impl.LastIndexOf(item, index, count, equalityComparer);

        public ImmutableList<T> Add(T value) => ImmutableList.CreateRange(_impl.Add(value));

        public ImmutableList<T> AddRange(IEnumerable<T> items) => ImmutableList.CreateRange(_impl.AddRange(items));

        public ImmutableList<T> Insert(int index, T element) => ImmutableList.CreateRange(_impl.Insert(index, element));

        public ImmutableList<T> InsertRange(int index, IEnumerable<T> items) 
            => ImmutableList.CreateRange(_impl.InsertRange(index, items));

        public ImmutableList<T> Remove(T value, IEqualityComparer<T> equalityComparer) 
            => ImmutableList.CreateRange(_impl.Remove(value, equalityComparer));
        
        public ImmutableList<T> Remove(T value) 
            => ImmutableList.CreateRange(_impl.Remove(value));

        public ImmutableList<T> RemoveAll(Predicate<T> match) => ImmutableList.CreateRange(_impl.RemoveAll(match));

        public ImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer) 
            => ImmutableList.CreateRange(_impl.RemoveRange(items, equalityComparer));

        public ImmutableList<T> RemoveRange(int index, int count) => ImmutableList.CreateRange(_impl.RemoveRange(index, count));

        public ImmutableList<T> RemoveAt(int index) => ImmutableList.CreateRange(_impl.RemoveAt(index));

        public ImmutableList<T> SetItem(int index, T value) => ImmutableList.CreateRange(_impl.SetItem(index, value));

        public ImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer) 
            => ImmutableList.CreateRange(_impl.Replace(oldValue, newValue, equalityComparer));

        /// <inheritdoc />
        public int Count => _impl.Count;
        
        public T this[int index] => _impl[index];
    }
}