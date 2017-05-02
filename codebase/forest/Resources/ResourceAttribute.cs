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

namespace Forest.Resources
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class ResourceAttribute : Attribute, IResource
    {
        private readonly string category;
        private readonly string bundle;
        private readonly string name;

        public ResourceAttribute(string category, string bundle, string name)
        {
            this.category = category;
            this.bundle = bundle;
            this.name = name;
        }

        public string Category { get { return category; } }
        public string Bundle { get { return bundle; } }
        public string Name { get { return name; } }

        public string Text { get; set; }
    }
}