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
using System.Collections.Generic;


namespace Forest.Collections
{
	public abstract class ReadOnlyBag<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> dictionary;

	    protected ReadOnlyBag(IDictionary<TKey, TValue> dictionary)
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
		protected IEnumerable<TKey> Keys { get { return dictionary.Keys; } }
		protected IEnumerable<TValue> Values { get { return dictionary.Values; } }

		public TValue this[TKey key] { get { return dictionary[key]; } }
	}
}