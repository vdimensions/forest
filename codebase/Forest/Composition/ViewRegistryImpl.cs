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
using System.Diagnostics;
using System.Reflection;

using Forest.Stubs;


namespace Forest.Composition
{
    internal sealed class ViewRegistryImpl : IViewRegistry
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly _ViewRegistry realRegistry;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IContainer container;

        private readonly IForestContext context;

        public ViewRegistryImpl(IForestContext context, _ViewRegistry realRegistry, IContainer container, Assembly configuratorAssembly)
        {

            this.context = context;
            this.realRegistry = realRegistry;
            this.container = container;
        }

        public IViewRegistry Register(Type viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException("viewType");
            }
            Register(context.GetDescriptor(viewType));
            return this;
        }
        public IViewRegistry Register(params Type[] viewTypes)
        {
            foreach (var viewType in viewTypes)
            {
                Register(viewType);
            }
            return this;
        }
        public IViewRegistry Register<T>() where T: IView
        {
            Register(context.GetDescriptor<T>());
            return this;
        }
        private void Register(IViewDescriptor descriptor)
        {
            realRegistry.Register(descriptor, container);
        }

        public IViewRegistry Unregister(Type viewType)
        {
            Unregister(context.GetDescriptor(viewType));
            return this;
        }
        public IViewRegistry Unregister<T>() where T: IView
        {
            Unregister(context.GetDescriptor<T>());
            return this;
        }
        private void Unregister(IViewDescriptor descriptor) { realRegistry.Unregister(descriptor.ViewAttribute.ID); }
    }
}