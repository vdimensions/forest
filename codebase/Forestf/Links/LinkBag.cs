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

using System.Collections;
using System.Collections.Generic;

using Forest.Collections;


namespace Forest.Links
{
    /// <summary>
    /// A class that serves as a read-only collection of <see cref="ILink">link</see> objects. 
    /// </summary>
    public sealed class LinkBag : ReadOnlyBag<string, ILink>, IEnumerable<ILink>
    {
        internal LinkBag(IDictionary<string, ILink> dictionary) : base(dictionary) { }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public IEnumerator<ILink> GetEnumerator() { return Values.GetEnumerator(); }

        new public IEnumerable<string> Keys { get { return base.Keys; } }
    }
}