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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


namespace Forest.Composition.Templates
{
    internal sealed class DefaultLayoutTemplateLoaderRegistry : ILayoutTemplateLoaderRegistry
    {
        private readonly ConcurrentDictionary<string, ILayoutTemplateLoader> data = new ConcurrentDictionary<string, ILayoutTemplateLoader>(StringComparer.OrdinalIgnoreCase);

        public IEnumerator<LayoutTemplateLoaderEntry> GetEnumerator() { return data.ToArray().Select(x => new LayoutTemplateLoaderEntry(x.Key, x.Value)).GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public ILayoutTemplateLoaderRegistry Register(string fileExtension, ILayoutTemplateLoader loader)
        {
            if (fileExtension == null)
            {
                throw new ArgumentNullException("fileExtension");
            }
            if (fileExtension.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "fileExtension");
            }
            if (loader == null)
            {
                throw new ArgumentNullException("loader");
            }
            if (fileExtension[0] == '.')
            {
                fileExtension = fileExtension.Substring(1);
            }
            if (!data.TryAdd(fileExtension, loader))
            {
                throw new InvalidOperationException(string.Format("A template loader has already been register for file extension '{0}'", fileExtension));
            }
            return this;
        }
    }
}