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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Forest.Commands;
using Forest.Composition;
using Forest.Events;
using Forest.Composition.Templates;
using Forest.Links;
using Forest.Resources;


namespace Forest
{
    partial class AbstractView<T> : IViewInit where T: class
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IViewContext viewContext;        
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ViewResolver viewResolver;

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private IRegion containingRegion;
		private RegionInfo containingRegionInfo;
        
        private IRegion GetOrCreateRegion(IRegionTemplate regionTemplate) 
        {
			var regionName = regionTemplate.RegionName;
            var region = regions.ContainsKey(regionName) ? regions[regionName] : null;
            if (region == null) 
            {
				var r = new Region (viewContext.ForestContext, regionTemplate, this, viewResolver);
				var p = r.OwnerView.ContainingRegion;
                var sb = new StringBuilder();
                if (p != null)
                {
                    sb.Append(p.Path);
                }
                r.Path = sb
                    .Append(viewContext.ForestContext.PathSeparator).Append(r.OwnerView.ID)
                    .Append(viewContext.ForestContext.PathSeparator).Append(r.Name)
                    .ToString();
				regions[regionName] = region = r;
            }
            return region;
        }
		IRegion IViewInit.GetOrCreateRegion(IRegionTemplate regionTemplate) { return GetOrCreateRegion(regionTemplate); }

        void IViewInit.TriggerInit() { OnInit(); }

        IViewContext IViewInit.Init(
            object viewModel,
			IForestContext context, 
			string id, 
			IRegion containingRegion, 
			IDictionary<string, IResource> resources,
            IDictionary<string, ILink> links,
            IDictionary<string, ICommand> commands,
			IDictionary<string, IRegion> regions,
			ViewResolver viewResolver)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (regions == null)
            {
                throw new ArgumentNullException(nameof(regions));
            }

            this.id = id;
            this.containingRegion = containingRegion;
			this.containingRegionInfo = containingRegion == null ? null : new RegionInfo(containingRegion);
            this.resourceBag = new ResourceBag(this.resources = resources);
            this.linkBag = new LinkBag(this.links = links);
            this.commandBag = new CommandBag(this.commands = commands);
            this.regionBag = new RegionBag(this.regions = regions);
			this.viewResolver = viewResolver;

            var viewContext = this.viewContext = new DefaultViewContext(this, context);
            var descriptor = viewContext.Descriptor;
            foreach (var resource in descriptor.ResourceAttributes)
            {
                AddResource(resource);
            }
            foreach (var link in descriptor.LinkToAttributes)
            {
                AddLink(link);
            }
            foreach (var command in descriptor.CommandMethods.Values)
            {
                AddCommand(command);
            }

            return this.viewContext;
        }

        void IViewInit.RegisterEventBus(IEventBus eventBus)
        {
            foreach (var subscriptionMethod in viewContext.Descriptor.SubscriptionMethods)
            {
                foreach (var topic in subscriptionMethod.Topics)
                {
                    eventBus.Subscribe(new SubscriptionHandler(this, subscriptionMethod), topic);
                }
            }
        }
        void IViewInit.OnEventBusReady(IEventBus eventBus)
        {
            if (eventBus == null)
            {
                throw new ArgumentNullException(nameof(eventBus));
            }
            OnEventBusReady(this.eventBus = eventBus);
        }
        void IViewInit.ReleaseEventBus()
        {
            if (eventBus == null)
            {
                return;
            }
            eventBus.Unsubscribe(this);
            eventBus = null;
        }

        IRegion IViewInit.ContainingRegion => this.containingRegion;
        IViewContext IViewInit.Context => this.viewContext;
    }
}