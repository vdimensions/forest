/*
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


namespace Forest.Composition
{
    /// <summary>
    /// An enum that represents the possible forms for a region layout.
    /// </summary>
    [Serializable]
    public enum RegionLayout
    {
        /// <summary>
        /// Layout for a region that can host multiple views, and more than one of them can be active at the same time.
        /// </summary>
        ManyActiveViews = 0,
        /// <summary>
        /// Layout for a region that can host multiple views, but only one of them can be active at a time.
        /// <remarks>This is the default region layout.</remarks>
        /// </summary>
        OneActiveView,
        /// <summary>
        /// Layout for a region that can host only one view, which is always active.
        /// </summary>
        SingleView,
        /// <summary>
        /// <see cref="ManyActiveViews" />
        /// </summary>
        Default = ManyActiveViews,
    }
}