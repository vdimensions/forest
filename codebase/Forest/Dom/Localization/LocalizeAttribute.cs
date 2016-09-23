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

using Forest.Stubs;


namespace Forest.Dom.Localization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class LocalizeAttribute : Attribute
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string bundle;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string name;

        [Localizable(false)]
        public LocalizeAttribute(string bundle, string name)
        {
            if (bundle == null)
            {
                throw new ArgumentNullException("bundle");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.bundle = bundle;
            this.name = name;
        }

        public string Bundle { get { return bundle; } }
        public string Name { get { return name; } }
        public ResourceInfo ResourceInfo { get { return new ResourceInfo(bundle, name);} }
    }
}