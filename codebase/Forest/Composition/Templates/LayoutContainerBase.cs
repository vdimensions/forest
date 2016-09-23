/*
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
using System.Diagnostics;
using System.Linq;

using Forest.Collections;
using Forest.Composition.Templates.Mutable;
using Forest.Stubs;


namespace Forest.Composition.Templates
{
    [Serializable]
    internal abstract class LayoutContainerBase<T> : IEnumerable<T> where T: class 
    {
        [Serializable]
        internal abstract class ContainerBucketBase : IEnumerable<T>
        {
            public abstract bool ContainsKey(string key);

            public abstract IEnumerator<T> GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            public abstract T this[string key] { get; }
        }

        [Serializable]
        private sealed class DefaultContainerBucket : ContainerBucketBase
        {
            private readonly ChronologicalDictionary<string, T> items = new ChronologicalDictionary<string, T>(StringComparer.Ordinal);

            public override IEnumerator<T> GetEnumerator() { return this.items.Values.GetEnumerator(); }

            public override bool ContainsKey(string key) { return this.items.ContainsKey(key); }

            public void Put(string key, T value) { items[key] = value; }

            public override T this[string key]
            {
                get
                {
                    T result;
                    return items.TryGetValue(key, out result) ? result : null;
                }
            }
        }

        private readonly IList<ContainerBucketBase> entries = new List<ContainerBucketBase>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IMutableLayoutTemplate ownerTemplate;

        protected LayoutContainerBase(IMutableLayoutTemplate ownerTemplate)
        {
            if (ownerTemplate == null)
            {
                throw new ArgumentNullException("ownerTemplate");
            }
            this.ownerTemplate = ownerTemplate;
        }

        public void Clear() { entries.Clear(); }

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "key");
            }
            return entries.Any(x => x.ContainsKey(key));
        }

        public IEnumerator<T> GetEnumerator() { return entries.SelectMany(x => x).GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }


        protected abstract void HandleExistingKey(IMutableLayoutTemplate ownerTemplate, string key);

        protected void AddEntry<TBase>(TBase entry) where TBase: LayoutContainerBase<T>.ContainerBucketBase { entries.Add(entry); }

        protected IMutableLayoutTemplate OwnerTemplate { get { return ownerTemplate; } }

        public IEnumerable Buckets { get { return entries; } }

        public T this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (key.Length == 0)
                {
                    throw new ArgumentException("Value cannot be empty string", "key");
                }
                return entries.Select(x => x[key]).FirstOrDefault(x => x != null);
            }
            set
            {
                if (ContainsKey(key))
                {
                    HandleExistingKey(ownerTemplate, key);
                }
                else
                {
                    DefaultContainerBucket container;
                    if ((container = entries.LastOrDefault() as DefaultContainerBucket) == null)
                    {
                        entries.Add(container = new DefaultContainerBucket());
                    }
                    container.Put(key, value);
                }
            }
        }
    }
}