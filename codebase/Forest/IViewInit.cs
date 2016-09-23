﻿/**
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
using System.ComponentModel;

using Forest.Composition;
using Forest.Events;


namespace Forest
{
    internal interface IViewInit
    {
        IViewContext Init(string id, IViewDescriptor descriptor, IRegion containingRegion, IDictionary<string, IRegion> childRegions);
        void RegisterEventBus(IEventBus eventBus);
        void OnEventBusReady(IEventBus eventBus);
        void ReleaseEventBus();

        IRegion ContainingRegion { get; }
        [Localizable(false)]
        IViewDescriptor Descriptor { get; }
        [Localizable(false)]
        IViewContext Context { get; }
    }
}