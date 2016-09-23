﻿/*
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
using System.Collections.Generic;
using System.Diagnostics;
using Forest.Composition;

namespace Forest.Commands
{
    public sealed class CommandResult
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IEnumerable<RegionModification> regionModifications;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object returnValue;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string navigateTo;

        public CommandResult(object returnValue, IEnumerable<RegionModification> regionModifications, string navigateTo)
        {
            this.returnValue = returnValue;
            this.regionModifications = regionModifications;
            this.navigateTo = navigateTo;
        }

        public object ReturnValue { get { return returnValue; } }
        public IEnumerable<RegionModification> RegionModifications { get { return regionModifications; } }
        public string NavigateTo { get { return navigateTo; } }
    }
}