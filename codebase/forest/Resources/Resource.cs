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
    [Serializable]
    public class Resource : IResource
    {
        private readonly string category;
        private readonly string bundle;
        private readonly string name;

        public Resource(string category, string bundle, string name)
        {
            this.category = category;
            this.bundle = bundle;
            this.name = name;
        }

        public string Category => category;
        public string Bundle => bundle;
        public string Name => name;
    }
}