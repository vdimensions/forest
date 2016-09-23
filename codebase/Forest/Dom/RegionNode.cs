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
using System.ComponentModel;
using System.Diagnostics;


namespace Forest.Dom
{
    [Serializable]
    internal class RegionNode : IRegionNode
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private readonly IDictionary<string, IViewNode> views;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string name;

        public RegionNode(string name, IDictionary<string, IViewNode> views)
        {
            this.name = name;
            this.views = views;
        }
        public RegionNode(string name) : this(name, null) { }

        public IEnumerator<KeyValuePair<string, IViewNode>> GetEnumerator() { return this.views.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        [Localizable(false)]
        public string Name { get { return this.name; } }

        public IViewNode this[string viewID] { get { return this.views[viewID]; } }
    }
}