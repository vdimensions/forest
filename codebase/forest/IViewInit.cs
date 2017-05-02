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
using Forest.Commands;
using Forest.Composition;
using Forest.Events;
using Forest.Composition.Templates;
using Forest.Links;
using Forest.Resources;


namespace Forest
{
    internal interface IViewInit
    {
		IRegion GetOrCreateRegion(IRegionTemplate regionTemplate);
		IViewContext Init(
            object viewModel,
			IForestContext context, 
			string id, 
			IRegion containingRegion, 
			IDictionary<string, IResource> resources, 
			IDictionary<string, ILink> links, 
			IDictionary<string, ICommand> commands, 
			IDictionary<string, IRegion> childRegions, 
			ViewResolver viewResolver);
        void RegisterEventBus(IEventBus eventBus);
        void OnEventBusReady(IEventBus eventBus);
        void ReleaseEventBus();
        void TriggerInit();

        [Localizable(false)]
        IRegion ContainingRegion { get; }

        [Localizable(false)]
        IViewContext Context { get; }
    }
}