using System.Collections;
using System.Collections.Generic;
using Axle.Verification;

namespace Forest.Collections.Immutable
{
    public static class ImmutableDictionary
    {
        public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>() 
            => new ImmutableDictionary<TKey, TValue>(System.Collections.Immutable.ImmutableDictionary.Create<TKey, TValue>());
        public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer) 
            => new ImmutableDictionary<TKey, TValue>(System.Collections.Immutable.ImmutableDictionary.Create<TKey, TValue>(keyComparer));
        public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
            => new ImmutableDictionary<TKey, TValue>(System.Collections.Immutable.ImmutableDictionary.CreateRange(keyComparer, items));

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> keyComparer)
        {
            Verifier.IsNotNull(Verifier.VerifyArgument(items, nameof(items)));
            Verifier.IsNotNull(Verifier.VerifyArgument(keyComparer, nameof(keyComparer)));
            return CreateRange(keyComparer, items);
        }
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Verifier.IsNotNull(Verifier.VerifyArgument(items, nameof(items)));
            return CreateRange(EqualityComparer<TKey>.Default, items);
        }
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            Verifier.IsNotNull(Verifier.VerifyArgument(dictionary, nameof(dictionary)));
            return CreateRange(dictionary.Comparer, dictionary);
        }
    }
    
    public class ImmutableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        public static readonly ImmutableDictionary<TKey, TValue> Empty = ImmutableDictionary.Create<TKey, TValue>();
        
        private readonly System.Collections.Immutable.ImmutableDictionary<TKey, TValue> _impl;

        internal ImmutableDictionary(System.Collections.Immutable.ImmutableDictionary<TKey, TValue> impl)
        {
            _impl = impl;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _impl.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public bool ContainsKey(TKey key) => _impl.ContainsKey(key);

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value) => _impl.TryGetValue(key, out value);
        
        public ImmutableDictionary<TKey, TValue> Clear() 
            => ImmutableDictionary.CreateRange(KeyComparer, _impl.Clear());

        public ImmutableDictionary<TKey, TValue> Add(TKey key, TValue value) 
            => ImmutableDictionary.CreateRange(KeyComparer, _impl.Add(key, value));

        public ImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs) 
            => ImmutableDictionary.CreateRange(KeyComparer, _impl.AddRange(pairs));

        public ImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value) 
            => ImmutableDictionary.CreateRange(KeyComparer, _impl.SetItem(key, value));

        public ImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items) 
            => ImmutableDictionary.CreateRange(KeyComparer, _impl.SetItems(items));

        public ImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys) 
            => ImmutableDictionary.CreateRange(KeyComparer, _impl.RemoveRange(keys));

        public ImmutableDictionary<TKey, TValue> Remove(TKey key) => ImmutableDictionary.CreateRange(KeyComparer, _impl.Remove(key));

        public bool Contains(KeyValuePair<TKey, TValue> pair) => _impl.Contains(pair);

        public bool TryGetKey(TKey equalKey, out TKey actualKey) => _impl.TryGetKey(equalKey, out actualKey);

        public TValue this[TKey key] => _impl[key];

        /// <inheritdoc />
        public IEnumerable<TKey> Keys => _impl.Keys;

        /// <inheritdoc />
        public IEnumerable<TValue> Values => _impl.Values;

        /// <inheritdoc />
        public int Count => _impl.Count;

        public IEqualityComparer<TKey> KeyComparer => _impl.KeyComparer;
    }
}