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
    internal sealed class RegionContainer : LayoutContainerBase<IMutableRegionTemplate>, IMutableRegionContainer
    {
        [Serializable]
        private sealed class InlinedTemplateContainer : ContainerBucketBase
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly IMutableLayoutTemplate template;

            public InlinedTemplateContainer(IMutableLayoutTemplate template)
            {
                this.template = template;
            }

            public override IEnumerator<IMutableRegionTemplate> GetEnumerator() { return template.Regions.Cast<IMutableRegionTemplate>().GetEnumerator(); }

            public override bool ContainsKey(string key) { return template.Regions[key] != null; }

            public override IMutableRegionTemplate this[string key] { get { return template.Regions[key]; } }
        }

        public RegionContainer(IMutableLayoutTemplate ownerTemplate) : base(ownerTemplate) { }

        protected override void HandleExistingKey(IMutableLayoutTemplate ownerTemplate, string key)
        {
            throw new LayoutTemplateException(ownerTemplate.ID, string.Format("Region '{0}' already defined.", key));
        }

        public void InlineTemplate(IMutableLayoutTemplate template) { AddEntry(new InlinedTemplateContainer(template)); }

        IEnumerator<IRegionTemplate> IEnumerable<IRegionTemplate>.GetEnumerator()
        {
            foreach (IMutableRegionTemplate regionTemplate in this)
            {
                yield return regionTemplate;
            }
        }

        IRegionTemplate ILayoutContainer<IRegionTemplate>.this[string key] { get { return base[key]; } }
    }
}