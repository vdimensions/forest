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
using System.Linq;

using Forest.Composition.Templates;


namespace Forest.Composition
{
    internal sealed class ViewResolver : _ViewResolver
    {
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private readonly _ViewRegistry viewRegistry;
        private readonly IForestContext context;

        public ViewResolver(IForestContext context, _ViewRegistry viewRegistry)
        {
            this.context = context;
            this.viewRegistry = viewRegistry;
        }

        private bool DoTryResolve(string id, object viewModel, IViewTemplate template, IRegion containingRegion, out Presenter presenter)
        {
            presenter = null;
            var entry = id == null 
                ? viewModel == null ? null : viewRegistry.Lookup(viewModel.GetType()) 
                : viewRegistry.Lookup(id.Substring(0, id.LastIndexOf('#')));
            if (entry == null)
            {
                return false;
            }
            var childRegions = template.Regions
                .Select(x => new Region(this.context, x, this))
                .ToDictionary(x => x.Name, x => x as IRegion, DefaultForestEngine.StringComparer);
                    
            var view = viewModel != null 
                ? entry.Container.ResolveView(entry.Type, id, viewModel)
                : entry.Container.ResolveView(entry.Type, id, entry.ViewModelType);
            ((IViewInit) view).Init(id, entry.Descriptor, containingRegion, childRegions);

            var resolvedPresenter = containingRegion == null 
                ? new Presenter(context, template, view) 
                : new Presenter(context, template, view, containingRegion);
            foreach (Region cr in childRegions.Values)
            {
                cr.Presenter = resolvedPresenter;
            }
            presenter = resolvedPresenter;
            return true;
        }

        private bool DoTryResolve(object viewModel, IViewContainer container, IRegion containingRegion, out Presenter presenter)
        {
            presenter = null;
            var entry = viewModel == null ? null : viewRegistry.Lookup(viewModel.GetType());
            if (entry == null)
            {
                return false;
            }
            var id = entry.ID;
            var template = container[id] ?? CreateViewTemplateOnTheFly(id);
            var childRegions = template.Regions
                .Select(x => new Region(context, x, this))
                .ToDictionary(x => x.Name, x => x as IRegion, DefaultForestEngine.StringComparer);

            var view = viewModel != null
                ? entry.Container.ResolveView(entry.Type, id, viewModel)
                : entry.Container.ResolveView(entry.Type, id, entry.ViewModelType);
            ((IViewInit) view).Init(id, entry.Descriptor, containingRegion, childRegions);

            var resolvedPresenter = containingRegion == null
                ? new Presenter(this.context, template, view)
                : new Presenter(this.context, template, view, containingRegion);
            foreach (Region cr in childRegions.Values)
            {
                cr.Presenter = resolvedPresenter;
            }
            presenter = resolvedPresenter;
            return true;
        }

        private IViewTemplate CreateViewTemplateOnTheFly(string id)
        {
            ILayoutTemplate t;
            if (context.LayoutTemplateProvider.TryLoad(id, out t))
            {
                return t;
            }
            return new QuickViewTemplate(id);
        }

        Presenter _ViewResolver.Resolve(string id, object viewModel, IViewTemplate template, IRegion containingRegion)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (id.Length == 0)
            {
                throw new ArgumentException("View ID must not be an empty string", "id");
            }
            Presenter view;
            if (DoTryResolve(id, viewModel, template, containingRegion, out view))
            {
                return view;
            }
            throw new ArgumentException(string.Format("Unable to resolve view for id '{0}'.", id), "id");
        }
        Presenter _ViewResolver.Resolve(object viewModel, IViewContainer template, IRegion containingRegion)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }
            Presenter view;
            if (DoTryResolve(viewModel, template, containingRegion, out view))
            {
                return view;
            }
            throw new ArgumentException(string.Format("Unable to resolve view for view model of type `{0}`.", viewModel.GetType()), "viewModel");
        }

        bool _ViewResolver.TryResolve(string id, object viewModel, IViewTemplate template, IRegion containingRegion, out Presenter view)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (id.Length == 0)
            {
                throw new ArgumentException("View ID must not be an empty string", "id");
            }
            return DoTryResolve(id, viewModel, template, containingRegion, out view);
        }
        bool _ViewResolver.TryResolve(object viewModel, IViewContainer template, IRegion containingRegion, out Presenter view)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }
            return DoTryResolve(viewModel, template, containingRegion, out view);
        }
    }
}