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
using System.ComponentModel;
using System.Diagnostics;


namespace Forest.Dom
{
    [Serializable]
    [Localizable(true)]
    public class ResourceNode : NavigationTarget, IResourceNode
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Localizable(false)] 
        private readonly string category;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Localizable(false)]
        private readonly string bundle;

        public ResourceNode(string category, string bundle, string name) : base(name)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }
            if (bundle == null)
            {
                throw new ArgumentNullException("bundle");
            }
            this.bundle = bundle;
        }
        
        public string Category { get { return category; } }
        public string Bundle { get { return bundle; } }
    }
}