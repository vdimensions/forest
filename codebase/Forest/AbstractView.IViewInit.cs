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

using Forest.Composition;
using Forest.Events;


namespace Forest
{
    partial class AbstractView<T> : IViewInit where T: class
    {
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private IViewDescriptor descriptor;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IViewContext context;

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private IRegion containingRegion;

        IViewContext IViewInit.Init(string id, IViewDescriptor descriptor, IRegion containingRegion, IDictionary<string, IRegion> regions)
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
            this.regions = regions;
            this.context = new DefaultViewContext(this.descriptor = descriptor, this);

            return this.context;
        }

        void IViewInit.RegisterEventBus(IEventBus eventBus)
        {
            foreach (var subscriptionMethod in this.descriptor.SubscriptionMethods)
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
        IViewContext IViewInit.Context { get { return this.context; } }
        IViewDescriptor IViewInit.Descriptor { get { return this.descriptor; } }
    }
}