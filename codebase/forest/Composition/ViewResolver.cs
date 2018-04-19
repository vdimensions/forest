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

using Forest.Collections;
using Forest.Commands;
using Forest.Composition.Templates;
using Forest.Engine;
using Forest.Links;
using Forest.Resources;


namespace Forest.Composition
{
    internal sealed class ViewResolver
    {
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
		private readonly IViewLookup _viewLookup;
		#if !DEBUG
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		#endif
        private readonly IForestContext _context;

        public ViewResolver(IForestContext context, IViewLookup viewLookup)
        {
            _context = context;
            _viewLookup = viewLookup;
        }

        private bool DoTryResolve(string id, object viewModel, IViewTemplate template, IRegion containingRegion, out IView view)
        {
			view = null;
            // TODO: remove ambiguity -- either access view by type or by id. Result false is not very consistent here
            var viewNumberPrefixIndex = id == null ? -1 : id.LastIndexOf('#');
            var token = id == null 
                ? viewModel == null ? null : this._viewLookup.Lookup(viewModel.GetType()) 
                : _viewLookup.Lookup(viewNumberPrefixIndex > 0 ? id.Substring(0, viewNumberPrefixIndex) : id);
            if (token == null)
            {
                return false;
            }
            var resolvedView = token.ResolveView(token.ViewType, id, viewModel);
            var childRegions = new Dictionary<string, IRegion>(DefaultForestEngine.StringComparer);
            var resources = new ChronologicalDictionary<string, IResource>(DefaultForestEngine.StringComparer);
            var links = new ChronologicalDictionary<string, ILink>(DefaultForestEngine.StringComparer);
            var commands = new ChronologicalDictionary<string, ICommand>(DefaultForestEngine.StringComparer);
            var viewInit = (IViewInit) resolvedView;
			viewInit.Init(viewModel, _context, id, containingRegion, resources, links, commands, childRegions, this);
			foreach (var regionTemplate in template.Regions) 
			{
				viewInit.GetOrCreateRegion(regionTemplate);
            }

            viewInit.TriggerInit();

            view = resolvedView;
            return true;
        }
		private bool DoTryResolve(object viewModel, IViewContainer container, IRegion containingRegion, out IView view)
        {
			view = null;
            var token = viewModel == null ? null : this._viewLookup.Lookup(viewModel.GetType());
            if (token == null)
            {
                return false;
            }
            var id = token.ID;
            var template = container[id] ?? CreateViewTemplateOnTheFly(id);

            var resolvedView = token.ResolveView(token.ViewType, id, viewModel);
            var childRegions = new Dictionary<string, IRegion>(DefaultForestEngine.StringComparer);
            var resources = new ChronologicalDictionary<string, IResource>(DefaultForestEngine.StringComparer);
            var links = new ChronologicalDictionary<string, ILink>(DefaultForestEngine.StringComparer);
            var commands = new ChronologicalDictionary<string, ICommand>(DefaultForestEngine.StringComparer);
            var viewInit = (IViewInit) resolvedView;
			viewInit.Init(viewModel, _context, id, containingRegion, resources, links, commands, childRegions, this);
			foreach (var regionTemplate in template.Regions) 
			{
				viewInit.GetOrCreateRegion(regionTemplate);
			}

            viewInit.TriggerInit();

            view = resolvedView;
            return true;
        }

        private IViewTemplate CreateViewTemplateOnTheFly(string id)
        {
            ILayoutTemplate t;
            if (_context.LayoutTemplateProvider.TryLoad(id, out t))
            {
                return t;
            }
            return new QuickViewTemplate(id);
        }

		public IView Resolve(string id, object viewModel, IViewTemplate template, IRegion containingRegion)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (id.Length == 0)
            {
                throw new ArgumentException("View ID must not be an empty string", "id");
            }
            if (DoTryResolve(id, viewModel, template, containingRegion, out var view))
            {
                return view;
            }
            throw new ArgumentException(string.Format("Unable to resolve view for id '{0}'.", id), nameof(id));
        }
        public IView Resolve(object viewModel, IViewContainer template, IRegion containingRegion)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }
			IView view;
            if (DoTryResolve(viewModel, template, containingRegion, out view))
            {
                return view;
            }
            throw new ArgumentException(string.Format("Unable to resolve view for view model of type `{0}`.", viewModel.GetType()), nameof(viewModel));
        }

		public bool TryResolve(string id, object viewModel, IViewTemplate template, IRegion containingRegion, out IView view)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (id.Length == 0)
            {
                throw new ArgumentException("View ID must not be an empty string", nameof(id));
            }
            return DoTryResolve(id, viewModel, template, containingRegion, out view);
        }
		public bool TryResolve(object viewModel, IViewContainer template, IRegion containingRegion, out IView view)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }
            return DoTryResolve(viewModel, template, containingRegion, out view);
        }
    }
}