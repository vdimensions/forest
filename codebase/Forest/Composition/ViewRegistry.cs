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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

using Forest.Stubs;


namespace Forest.Composition
{
    internal sealed class ViewRegistry : _ViewRegistry
    {
        private struct ViewEntry : IViewEntry
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly string id;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly IContainer container;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly IViewDescriptor descriptor;

            public ViewEntry(string id, IContainer container, IViewDescriptor descriptor)
            {
                this.id = id;
                this.container = container;
                this.descriptor = descriptor;
            }

            public string ID { get { return id; } }
            public Type Type { get { return descriptor.ViewType; } }
            public Type ViewModelType { get { return descriptor.ViewModelType; } }
            public IContainer Container { get { return container; } }
            public IViewDescriptor Descriptor { get { return descriptor; } }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ILogger logger;

        private readonly ConcurrentDictionary<string, IViewEntry> registry = new ConcurrentDictionary<string, IViewEntry>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<Type, IViewEntry> reverseViewModelToViewMap = new ConcurrentDictionary<Type, IViewEntry>();
        private readonly IForestContext context;

        public ViewRegistry(IForestContext context)
        {
            this.context = context;
            this.logger = context.LoggerFactory.GetLogger<ViewRegistry>();
        }

        #region Implementation of _ViewRegistry
        public IViewEntry Lookup(string id)
        {
            IViewEntry result;
            return registry.TryGetValue(id, out result) ? result : null;
        }
        public IViewEntry Lookup(Type viewModelType)
        {
            IViewEntry result;
            if (!reverseViewModelToViewMap.TryGetValue(viewModelType, out result))
            {
                var candidatePairs = reverseViewModelToViewMap.Where(x => x.Key.IsAssignableFrom(viewModelType)).ToArray();
                if (candidatePairs.Length == 1)
                {
                     return candidatePairs[0].Value;
                }
            }
            return null;
        }

        public void Register(string id, Type viewType, IContainer container)
        {
            var entry = new ViewEntry(id, container, this.context.GetDescriptor(viewType));
            if (registry.TryAdd(id, entry))
            {
                reverseViewModelToViewMap.TryAdd(entry.ViewModelType, entry);
                //logger.Warn("A view with id '{0}' has already been configured. Overwriting the existing data.", id);
            }
        }
        public void Register(IViewDescriptor descriptor, IContainer container)
        {
            var id = descriptor.ViewAttribute.ID;
            var entry = new ViewEntry(id, container, descriptor);
            if (registry.TryAdd(id, entry))
            {
                reverseViewModelToViewMap.TryAdd(entry.ViewModelType, entry);
                //logger.Warn("A view with id '{0}' has already been configured. Overwriting the existing data.", id);
            }
        }

        public void Unregister(string id)
        {
            IViewEntry entry;
            if (!registry.TryRemove(id, out entry))
            {
                logger.Warn("Attemting to remove a non-registered view id: \"{0}\"", id);
                IViewEntry idToRemove;
                reverseViewModelToViewMap.TryRemove(entry.ViewModelType, out idToRemove);
            }
        }
        #endregion
    }
}