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
        [Obsolete]
        private readonly string _category;
        private readonly string _bundle;
        private readonly string _name;

        public Resource(string category, string bundle, string name)
        {
            _category = category;
            _bundle = bundle;
            _name = name;
        }

        [Obsolete]
        public string Category => _category;
        public string Bundle => _bundle;
        public string Name => _name;
    }
}