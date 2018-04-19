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


namespace Forest.Stubs
{
    [Serializable]
    public struct ResourceInfo : IEquatable<ResourceInfo>
    {
        public static readonly ResourceInfo Empty = new ResourceInfo();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string bundle;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string key;

        public ResourceInfo(string bundle, string key) : this()
        {
            this.key = key;
            this.bundle = bundle;
        }

        public ResourceInfo ChangeKey(string format, params object[] args)
        {
            return args.Length == 0
                ? new ResourceInfo(bundle, format)
                : new ResourceInfo(bundle, string.Format(format, args));
        }

        public override bool Equals(object other) { return other is ResourceInfo && Equals((ResourceInfo) other); }
        public bool Equals(ResourceInfo other)
        {
            var comparer = StringComparer.Ordinal;
            return comparer.Equals(other.bundle, bundle) && comparer.Equals(other.key, key);
        }

        public override int GetHashCode() { return bundle.GetHashCode()^key.GetHashCode(); }

        public string Bundle => bundle ?? string.Empty;
        public string Key => key ?? string.Empty;
    }
}