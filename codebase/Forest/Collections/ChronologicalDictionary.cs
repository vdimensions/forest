using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;


namespace Forest.Collections
{
    /// <summary>
    /// Represents a collection of key/value pairs that are sorted by the time of insertion (chronologically)
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary. </typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <remarks>This class cannot be inherited.</remarks>
    [Serializable]
    internal sealed class ChronologicalDictionary<TKey, TValue> : DictionaryProxy<TKey, TValue>
    {
        [Serializable]
        internal struct ChronologicalKey<TKey> : IEquatable<ChronologicalKey<TKey>>
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly TKey key;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly long timestamp;

            public ChronologicalKey(TKey key, DateTime dateTime) : this()
            {
                this.key = key;
                this.timestamp = dateTime.Ticks;
            }

            public override int GetHashCode() { return this.key.GetHashCode(); }
            public bool Equals(ChronologicalKey<TKey> other) { return Equals(this.Key, other.Key); }

            public TKey Key { get { return this.key; } }
            public long Timestamp { get { return this.timestamp; } }
        }

        [Serializable]
        internal sealed class ChronologicalKeyComparer<TKey> : IComparer<ChronologicalKey<TKey>>
        {
            public ChronologicalKeyComparer()
            {
            }

            public int Compare(ChronologicalKey<TKey> x, ChronologicalKey<TKey> y)
            {
                return x.Timestamp.CompareTo(y.Timestamp);
            }
        }

        [Serializable]
        internal sealed class ChronologicalKeyEqualityComparer<TKey> : IEqualityComparer<ChronologicalKey<TKey>>
        {
            private readonly IEqualityComparer<TKey> keyComparer;

            public ChronologicalKeyEqualityComparer() : this(EqualityComparer<TKey>.Default) { }
            public ChronologicalKeyEqualityComparer(IEqualityComparer<TKey> keyComparer)
            {
                if (keyComparer == null)
                {
                    throw new ArgumentNullException("keyComparer");
                }
                this.keyComparer = keyComparer;
            }

            public bool Equals(ChronologicalKey<TKey> x, ChronologicalKey<TKey> y)
            {
                return this.keyComparer.Equals(x.Key, y.Key);
            }

            public int GetHashCode(ChronologicalKey<TKey> obj) { return this.keyComparer.GetHashCode(obj.Key); }
        }

        [Serializable]
        private sealed class TimestampDictionary : Dictionary<ChronologicalKey<TKey>, TValue>, IDictionary<TKey, TValue>, ISerializable
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            [NonSerialized]
            private readonly ICollection<KeyValuePair<ChronologicalKey<TKey>, TValue>> collection;

            private TimestampDictionary(IDictionary<ChronologicalKey<TKey>, TValue> dictionary) : base(dictionary)
            {
                this.collection = this;
            }
            public TimestampDictionary() : this(
                new Dictionary<ChronologicalKey<TKey>, TValue>(new ChronologicalKeyEqualityComparer<TKey>())) { }
            public TimestampDictionary(int capacity) : this(
                new Dictionary<ChronologicalKey<TKey>, TValue>(capacity, new ChronologicalKeyEqualityComparer<TKey>())) { }
            public TimestampDictionary(int capacity, IEqualityComparer<TKey> comparer) : this(
                new Dictionary<ChronologicalKey<TKey>, TValue>(capacity, new ChronologicalKeyEqualityComparer<TKey>(comparer))) { }
            public TimestampDictionary(IEqualityComparer<TKey> comparer) : this(
                new Dictionary<ChronologicalKey<TKey>, TValue>(new ChronologicalKeyEqualityComparer<TKey>(comparer))) { }

            #region Serialization Support
            internal TimestampDictionary(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
            {
                this.collection = this;
            }
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                GetObjectData(info, context);
            }
            #endregion

            private IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
            {
                return this.collection
                    .OrderBy(x => x.Key, new ChronologicalKeyComparer<TKey>())
                    .Select(x => new KeyValuePair<TKey, TValue>(x.Key.Key, x.Value));
            }

            #region Implementation of IEnumerable<KeyValuePair<TKey,TValue>>
            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() { return Enumerate().GetEnumerator(); }
            #endregion

            #region Implementation of ICollection<KeyValuePair<TKey,TValue>>
            void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
            {
                var k = new ChronologicalKey<TKey>(item.Key, DateTime.UtcNow);
                var kvp = new KeyValuePair<ChronologicalKey<TKey>, TValue>(k, item.Value);
                this.collection.Add(kvp);
            }

            bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            {
                var k = new ChronologicalKey<TKey>(item.Key, DateTime.UtcNow);
                var kvp = new KeyValuePair<ChronologicalKey<TKey>, TValue>(k, item.Value);
                return this.collection.Contains(kvp);
            }

            void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                base.Keys.Select(x => new KeyValuePair<TKey, TValue>(x.Key, this[x])).ToList().CopyTo(array, arrayIndex);
            }

            bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
            {
                var k = new ChronologicalKey<TKey>(item.Key, DateTime.UtcNow);
                var kvp = new KeyValuePair<ChronologicalKey<TKey>, TValue>(k, item.Value);
                return this.collection.Remove(kvp);
            }

            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly { get { return this.collection.IsReadOnly; } }
            #endregion

            #region Implementation of IDictionary<TKey,TValue>
            public bool ContainsKey(TKey key)
            {
                var k = new ChronologicalKey<TKey>(key, DateTime.UtcNow);
                return ContainsKey(k);
            }

            public void Add(TKey key, TValue value)
            {
                var k = new ChronologicalKey<TKey>(key, DateTime.UtcNow);
                Add(k, value);
            }

            public bool Remove(TKey key)
            {
                var k = new ChronologicalKey<TKey>(key, DateTime.UtcNow);
                return Remove(k);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                var k = new ChronologicalKey<TKey>(key, DateTime.UtcNow);
                return TryGetValue(k, out value);
            }

            public TValue this[TKey key]
            {
                get
                {
                    var k = new ChronologicalKey<TKey>(key, DateTime.UtcNow);
                    return base[k];
                }
                set
                {
                    var k = new ChronologicalKey<TKey>(key, DateTime.UtcNow);
                    Remove(k);
                    base[k] = value;
                }
            }

            new private ICollection<TKey> Keys { get { return Enumerate().Select(key => key.Key).ToArray(); } }
            ICollection<TKey> IDictionary<TKey, TValue>.Keys { get { return this.Keys; } }

            new private ICollection<TValue> Values { get { return Enumerate().Select(x => x.Value).ToArray(); } }
            ICollection<TValue> IDictionary<TKey, TValue>.Values { get { return this.Values; } }
            #endregion
        }

        public ChronologicalDictionary() : base(new TimestampDictionary()) { }
        public ChronologicalDictionary(IEqualityComparer<TKey> comparer) : base(new TimestampDictionary(comparer)) { }
        public ChronologicalDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(new TimestampDictionary(capacity, comparer)) { }
        public ChronologicalDictionary(int capacity) : base(new TimestampDictionary(capacity)) { }
    }
}