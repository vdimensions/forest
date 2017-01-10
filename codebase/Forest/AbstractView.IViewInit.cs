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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Forest.Composition;
using Forest.Events;
using Forest.Composition.Templates;


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

				var ids = new LinkedList<string>();
				ids.AddFirst(r.Name);
				ids.AddFirst(r.OwnerView.ID);
				var p = r.OwnerView.ContainingRegion;
				//while (p != null)
				//{
				//	ids.AddFirst(p.Name);
                //    ids.AddFirst(p.OwnerView.ID);
				//	p = p.OwnerView.ContainingRegion;
				//}
				if (p != null) 
				{
					ids.AddFirst(p.Path);
				}
				r.Path = ids.Aggregate(new StringBuilder(), (sb, x) => sb.Append(viewContext.ForestContext.PathSeparator).Append(x)).ToString();

				regions[regionName] = region = r;
            }
            return region;
        }
		IRegion IViewInit.GetOrCreateRegion(IRegionTemplate regionTemplate) { return GetOrCreateRegion(regionTemplate); }

        IViewContext IViewInit.Init(
			IForestContext context, 
			string id, 
			IViewDescriptor descriptor, 
			IRegion containingRegion, 
			IDictionary<string, IRegion> regions, 
			ViewResolver viewResolver)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (regions == null)
            {
                throw new ArgumentNullException("regions");
            }
            this.id = id;
            this.containingRegion = containingRegion;
			this.containingRegionInfo = containingRegion == null ? null : new RegionInfo(containingRegion);
			this.regionBag = new RegionBag(this.regions = regions);
			this.viewResolver = viewResolver;
			return this.viewContext = new DefaultViewContext(descriptor, this, context);
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
                throw new ArgumentNullException("eventBus");
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

        IRegion IViewInit.ContainingRegion { get { return this.containingRegion; } }
        IViewContext IViewInit.Context { get { return this.viewContext; } }
    }
}