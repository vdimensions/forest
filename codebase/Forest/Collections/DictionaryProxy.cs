/**
 * Copyright 2014 vdimensions.net.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections;
using System.Collections.Generic;


namespace Forest.Collections
{
	public class ReadOnlyDictionary<TKey, TValue> //: IEnumerable<KeyValuePair<TKey, TValue>> 
	{
		private readonly IDictionary<TKey, TValue> dictionary;

		public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null) 
			{
			    throw new ArgumentNullException ("dictionary");
			}
			this.dictionary = dictionary;
		}

		//IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		//public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return dictionary.GetEnumerator(); }

		public bool TryGetValue(TKey key, out TValue value) { return dictionary.TryGetValue(key, out value); }

		public int Count { get { return dictionary.Count; } }
		public IEnumerable<TKey> Keys { get { return dictionary.Keys; } }
		public IEnumerable<TValue> Values { get { return dictionary.Values; } }

		public TValue this[TKey key] { get { return dictionary[key]; } }
	}

    [Serializable]
    public abstract class DictionaryProxy<TKey, TValue> : MarshalByRefObject, IDictionary<TKey, TValue>
    {
        protected IDictionary<TKey, TValue> Target;

        protected DictionaryProxy(IDictionary<TKey, TValue> target) { this.Target = target; }

        #region Implementation of IEnumerable
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return this.Target.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        #endregion

        #region Implementation of ICollection<KeyValuePair<TKey,TValue>>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) { Add(item); }
        protected virtual void Add(KeyValuePair<TKey, TValue> item) { this.Target.Add(item); }

        public virtual void Clear() { this.Target.Clear(); }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) { return Contains(item); }
        protected virtual bool Contains(KeyValuePair<TKey, TValue> item) { return this.Target.Contains(item); }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { CopyTo(array, arrayIndex); }
        protected virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { this.Target.CopyTo(array, arrayIndex); }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) { return Remove(item); }
        protected virtual bool Remove(KeyValuePair<TKey, TValue> item) { return this.Target.Remove(item); }

        public virtual int Count { get { return this.Target.Count; } }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly { get { return this.Target.IsReadOnly; } }
        protected virtual bool IsReadOnly { get { return this.Target.IsReadOnly; } }
        #endregion

        #region Implementation of IDictionary<TKey,TValue>
        public virtual bool ContainsKey(TKey key) { return this.Target.ContainsKey(key); }

        public virtual void Add(TKey key, TValue value) { this.Target.Add(key, value); }

        public virtual bool Remove(TKey key) { return this.Target.Remove(key); }

        public virtual bool TryGetValue(TKey key, out TValue value) { return this.Target.TryGetValue(key, out value); }

        public virtual TValue this[TKey key]
        {
            get { return this.Target[key]; }
            set { this.Target[key] = value; }
        }

        public virtual ICollection<TKey> Keys { get { return this.Target.Keys; } }

        public virtual ICollection<TValue> Values { get { return this.Target.Values; } }
        #endregion
    }
}