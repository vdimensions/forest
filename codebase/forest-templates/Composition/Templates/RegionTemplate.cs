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

using Forest.Composition.Templates.Mutable;


namespace Forest.Composition.Templates
{
    [Serializable]
    internal class RegionTemplate : LayoutContainerBase<IMutableViewTemplate>, IMutableRegionTemplate
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string regionName;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly RegionLayout layout;

        public RegionTemplate(string regionName, RegionLayout layout, IMutableLayoutTemplate ownerTemplate) : base(ownerTemplate)
        {
            this.regionName = regionName;
            this.layout = layout;
        }

        public void AddPlaceholder(IMutablePlaceholder placeholder)
        {
            AddEntry(new Placeholder.PlaceholderBucket(placeholder));
            OwnerTemplate.Placeholders[placeholder.ID] = placeholder;
        }

        IEnumerator<IViewTemplate> IEnumerable<IViewTemplate>.GetEnumerator()
        {
            foreach (IMutableViewTemplate viewTemplate in this)
            {
                yield return viewTemplate;
            }
        }

        protected override void HandleExistingKey(IMutableLayoutTemplate ownerTemplate, string key)
        {
            throw new LayoutTemplateException(ownerTemplate.ID, string.Format("View '{0}' is already defined in region '{1}'.", key, regionName));
        }

        public string RegionName { get { return regionName; } }
        public RegionLayout Layout { get { return layout; } }

        IViewTemplate ILayoutContainer<IViewTemplate>.this[string key] { get { return base[key]; } }
    }
}