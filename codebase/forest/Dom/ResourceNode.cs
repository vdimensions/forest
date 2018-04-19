﻿/**
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
        private readonly string _category;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Localizable(false)]
        private readonly string _bundle;

        public ResourceNode(string category, string bundle, string name) : base(name)
        {
            _bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
            _category = category ?? throw new ArgumentNullException(nameof(category));
        }
        
        public string Category => _category;
        public string Bundle => _bundle;
    }
}