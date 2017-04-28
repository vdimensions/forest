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
using System.Diagnostics;
using System.Linq;

using Forest.Composition.Templates.Mutable;


namespace Forest.Composition.Templates
{
    [Serializable]
    internal sealed class Placeholder : LayoutContainerBase<IMutableViewTemplate>, IMutablePlaceholder
    {
        [Serializable]
        internal sealed class PlaceholderBucket : ContainerBucketBase
        {
            private readonly IMutablePlaceholder placeholder;

            public PlaceholderBucket(IMutablePlaceholder placeholder) { this.placeholder = placeholder; }

            public override IEnumerator<IMutableViewTemplate> GetEnumerator() { return placeholder.Cast<IMutableViewTemplate>().GetEnumerator(); }

            public override bool ContainsKey(string key) { return placeholder.ContainsKey(key); }

            public override IMutableViewTemplate this[string key] { get { return placeholder[key]; } }
            public IMutablePlaceholder Placeholder { get { return placeholder; } }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string id;

        public Placeholder(string id, IMutableLayoutTemplate ownerTemplate) : base(ownerTemplate)
        {
            this.id = id;
        }

        protected override void HandleExistingKey(IMutableLayoutTemplate ownerTemplate, string key)
        {
            throw new LayoutTemplateException(ownerTemplate.ID, string.Format("Placeholder '{0}' already contains entry with id '{1}' ", id, key));
        }

        IEnumerator<IViewTemplate> IEnumerable<IViewTemplate>.GetEnumerator()
        {
            foreach (var vt in this)
            {
                yield return vt;
            }
        }

        public void AddPlaceholder(IMutablePlaceholder placeholder)
        {
            /*
            if (key.Equals(this.ID, StringComparison.Ordinal))
            {
                OwnerTemplate.Placeholders.Remove(this.ID);
            }*/
            OwnerTemplate.Placeholders.Remove(ID);
            AddEntry(new PlaceholderBucket(placeholder));
        }

        public string ID { get { return id; } }

        IViewTemplate ILayoutContainer<IViewTemplate>.this[string key] { get { return base[key]; } }
    }
}