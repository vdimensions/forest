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
        private readonly string _bundle;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _key;

        public ResourceInfo(string bundle, string key) : this()
        {
            _key = key;
            _bundle = bundle;
        }

        public ResourceInfo ChangeKey(string format, params object[] args)
        {
            return args.Length == 0
                ? new ResourceInfo(_bundle, format)
                : new ResourceInfo(_bundle, string.Format(format, args));
        }

        public override bool Equals(object other) { return other is ResourceInfo info && Equals(info); }
        public bool Equals(ResourceInfo other)
        {
            var comparer = StringComparer.Ordinal;
            return comparer.Equals(other._bundle, _bundle) && comparer.Equals(other._key, _key);
        }

        public override int GetHashCode() { return _bundle.GetHashCode()^_key.GetHashCode(); }

        public string Bundle => _bundle ?? string.Empty;
        public string Key => _key ?? string.Empty;
    }
}