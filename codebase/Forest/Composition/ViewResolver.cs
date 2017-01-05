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
using System.Linq;

using Forest.Composition.Templates;
using Forest.Engine;
using System.Collections.Generic;


namespace Forest.Composition
{
    internal sealed class ViewResolver
    {
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
		private readonly IViewLookup viewLookup;
		#if !DEBUG
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		#endif
        private readonly IForestContext context;

        public ViewResolver(IForestContext context, IViewLookup viewLookup)
        {
            this.context = context;
            this.viewLookup = viewLookup;
        }

        private bool DoTryResolve(string id, object viewModel, IViewTemplate template, IRegion containingRegion, out Presenter presenter)
        {
            presenter = null;
            // TODO: remove ambiguity -- either access view by type or by id. Result false is not very consistent here
            var viewNumberPrefixIndex = id == null ? -1 : id.LastIndexOf('#');
            var token = id == null 
                ? viewModel == null ? null : this.viewLookup.Lookup(viewModel.GetType()) 
                : this.viewLookup.Lookup(viewNumberPrefixIndex > 0 ? id.Substring(0, viewNumberPrefixIndex) : id);
            if (token == null)
            {
                return false;
            }
            var viewDescriptor = this.context.GetDescriptor(token.ViewType);
            var view = token.ResolveView(token.ViewType, id, viewModel);
			IViewInit viewInit = (IViewInit) view;
			var childRegions = new Dictionary<string, IRegion> (DefaultForestEngine.StringComparer);
			viewInit.Init(context, id, viewDescriptor, containingRegion, childRegions, this);
			var resolvedPresenter = new Presenter(context, template, view, containingRegion);
			foreach (Region region in template.Regions.Select(x => viewInit.GetOrCreateRegion(x))) 
			{
				region.Presenter = resolvedPresenter;
				childRegions.Add(region.Name, region);
			}

            presenter = resolvedPresenter;
            return true;
        }
        private bool DoTryResolve(object viewModel, IViewContainer container, IRegion containingRegion, out Presenter presenter)
        {
            presenter = null;
            var token = viewModel == null ? null : this.viewLookup.Lookup(viewModel.GetType());
            if (token == null)
            {
                return false;
            }
            var id = token.ID;
            var template = container[id] ?? CreateViewTemplateOnTheFly(id);

            var viewDescriptor = this.context.GetDescriptor(token.ViewType);
            var view = token.ResolveView(token.ViewType, id, viewModel);
			IViewInit viewInit = (IViewInit) view;
			var childRegions = new Dictionary<string, IRegion> (DefaultForestEngine.StringComparer);
			viewInit.Init(context, id, viewDescriptor, containingRegion, childRegions, this);
			var resolvedPresenter = new Presenter(context, template, view, containingRegion);
			foreach (Region region in template.Regions.Select(x => viewInit.GetOrCreateRegion(x))) 
			{
				region.Presenter = resolvedPresenter;
				childRegions.Add(region.Name, region);
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

        public Presenter Resolve(string id, object viewModel, IViewTemplate template, IRegion containingRegion)
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
        public Presenter Resolve(object viewModel, IViewContainer template, IRegion containingRegion)
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

        public bool TryResolve(string id, object viewModel, IViewTemplate template, IRegion containingRegion, out Presenter view)
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
        public bool TryResolve(object viewModel, IViewContainer template, IRegion containingRegion, out Presenter view)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }
            return DoTryResolve(viewModel, template, containingRegion, out view);
        }
    }
}