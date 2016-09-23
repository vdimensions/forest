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
using System.Diagnostics;

using Forest.Composition.Templates.Mutable;


namespace Forest.Composition.Templates
{
    [Serializable]
    internal class ViewTemplate : IMutableViewTemplate
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string id;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IMutableLayoutTemplate ownerTemplate;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IMutableRegionContainer regionContainer;

        public ViewTemplate(string id, IMutableLayoutTemplate template)
        {
            this.id = id;
            this.ownerTemplate = template;
            this.regionContainer = new RegionContainer(template);
        }

        public string ID { get { return id; } }

        public IMutableRegionContainer Regions { get { return regionContainer; } }
        IRegionContainer IViewTemplate.Regions { get { return Regions; } }

        public IMutableLayoutTemplate Template { get { return ownerTemplate; } }
    }
}