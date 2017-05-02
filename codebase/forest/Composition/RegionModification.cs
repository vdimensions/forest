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

namespace Forest.Composition
{
    [Serializable]
    public struct RegionModification : IEquatable<RegionModification>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string regionPath;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string viewID;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly RegionModificationType modificationType;

        public RegionModification(string regionPath, string viewID, RegionModificationType modificationType)
        {
            if (regionPath == null)
            {
                throw new ArgumentNullException("regionPath");
            }
            if (viewID == null)
            {
                throw new ArgumentNullException("viewID");
            }
            this.regionPath = regionPath;
            this.viewID = viewID;
            this.modificationType = modificationType;
        }

        public override bool Equals(object obj) { return obj is RegionModification && Equals((RegionModification) obj); }
        public bool Equals(RegionModification other)
        {
            const StringComparison comparison = StringComparison.Ordinal;
            return string.Equals(regionPath, other.regionPath, comparison)
                && string.Equals(viewID, other.viewID, comparison)
                && (modificationType == other.modificationType);
        }

        public override int GetHashCode() { return regionPath.GetHashCode() ^ modificationType.GetHashCode() ^ viewID.GetHashCode(); }

        public string RegionPath { get { return regionPath; } }
        public string ViewID { get { return viewID; } }
        public RegionModificationType ModificationType { get { return modificationType; } }
    }
}