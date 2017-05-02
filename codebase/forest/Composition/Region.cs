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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Forest.Collections;
using Forest.Composition.Templates;
using Forest.Stubs;


namespace Forest.Composition
{
    [Localizable(false)]
    internal sealed class Region : IRegion, IRegionUtil
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ILogger logger;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object syncRoot = new object();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IView ownerView;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ChronologicalDictionary<string, IView> activeViews;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly ViewBag activeViewsBag;
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private readonly ChronologicalDictionary<string, IView> allViews;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly ViewBag allViewsBag;
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private readonly IRegionTemplate template;
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private readonly ViewResolver resolver;

		#if !DEBUG
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		#endif
		internal readonly IForestContext context;

        public Region(IForestContext context, IRegionTemplate template, IView ownerView, ViewResolver resolver) 
		    : this(context, template, ownerView, resolver, StringComparer.Ordinal) { }
        private Region(IForestContext context, IRegionTemplate template, IView ownerView, ViewResolver resolver, IEqualityComparer<string> comparer)
        {
            this.context = context;
            this.logger = context.LoggerFactory.GetLogger<Region>();
			this.activeViewsBag = new ViewBag(this.activeViews = new ChronologicalDictionary<string, IView>(comparer));
			this.allViewsBag = new ViewBag(this.allViews = new ChronologicalDictionary<string, IView>(comparer));
            this.template = template;
			this.ownerView = ownerView;
            this.resolver = resolver;
        }

        [Obsolete]
        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        public IView Populate(string id, int? index, object viewModel, bool load)
        {
            IView result;
            var viewKey = id;
            if (index.HasValue)
            {
				viewKey = string.Format("{0}#{1}", id, index.Value);
            }
			var viewTemplate = template[id];
            if (viewTemplate == null)
            {
                logger.Trace("Region '{0}' did not have explicit definition for child view '{1}'. Creating view template on the fly.", Path, id);
                viewTemplate = CreateViewTemplateOnTheFly(id);
            }
            lock (syncRoot)
            {
                if ((viewModel != null) && allViews.ContainsKey(id))
                {
                    // in case we are replacing the viewModel, then we must re-initialize the view

                    var view = allViews[viewKey];
                    allViews.Remove(viewKey);
                    view.Dispose();
                }

                if ((Layout == RegionLayout.OneActiveView) || (Layout == RegionLayout.SingleView))
                {
                    foreach (var av in ActiveViews)
                    {
                        DoDeactivateView(av.ID);
                    }
                    if (Layout == RegionLayout.SingleView)
                    {
                        foreach (var v in allViews.Values)
                        {
                            v.Dispose();
                        }
                        allViews.Clear();
                    }
                }
                
                if (!allViews.TryGetValue(viewKey, out result))
                {
					IView view;
                    if (resolver.TryResolve(id, viewModel, viewTemplate, this, out view))
                    {
                        allViews.Add(viewKey, result = view);
                        if (load)
                        {
                            result.Load();
                        }
                    }
                    else
                    {
                        #warning no view found, throw exception
                        //BUG no view found, throw exception
                    }
                }

                if (result != null)
                {
                    activeViews[viewKey] = result;
                    result.Refreshed += OnViewRefreshed;
					OnViewAdded(result);
                }
                
                logger.Debug("View '{0}' has been activated inside region '{1}'", id, Path);
            }
            return result;
        }

        [Obsolete]
        public IView Populate(object viewModel, int? index, bool load)
        {
            IView result = null;
			IView view = null;
            if (resolver.TryResolve(viewModel.GetType(), template, this, out view))
            {
                var id = view.ID;
                var viewKey = id;
                if (index.HasValue)
                {
                    viewKey = string.Format("{0}#{1}", viewKey, index.Value);
                }
                lock (syncRoot)
                {
                    if (allViews.ContainsKey(viewKey))
                    {
                        // in case we are replacing the viewModel, then we must re-initialize the view
                        var v = allViews[viewKey];
                        allViews.Remove(viewKey);
                        v.Dispose();
                    }

                    if ((Layout == RegionLayout.OneActiveView) || (Layout == RegionLayout.SingleView))
                    {
                        foreach (var av in ActiveViews)
                        {
                            DoDeactivateView(av.ID);
                        }
                        if (Layout == RegionLayout.SingleView)
                        {
                            foreach (var v in this.allViews.Values)
                            {
                                v.Dispose();
                            }
                            allViews.Clear();
                        }
                    }
                    allViews.Add(viewKey, result = view);
                    if (load)
                    {
                        result.Load();
                    }
                    activeViews[viewKey] = result;
                    result.Refreshed += OnViewRefreshed;
					OnViewAdded(result);
                    logger.Debug("View '{0}' has been activated inside region '{1}'", id, Path);
                }
            }
            else
            {
                #warning no view found, throw exception
                //BUG no view found, throw exception
            }
            return result;
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

        void OnViewRefreshed(IView view)
        {
            var cch = ContentChange;
            if (cch != null)
            {
                cch(this, view, RegionModificationType.RefreshRequested);
            }
        }

		void OnViewAdded(IView view)
		{
			var cch = ContentChange;
			if (cch != null)
			{
				cch(this, view, RegionModificationType.ViewAdded);
			}
		}

        public IView ActivateView(string id) { return Populate(id, null, null, true); }
        public IView ActivateView(string id, object viewModel) { return Populate(id, null, viewModel, true); }
        public IView ActivateView(string id, int index, object viewModel)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }
            return Populate(id, index, viewModel, true);
        }
        public TView ActivateView<TView>(string id) where TView: IView
        {
            return (TView) ActivateView(id);
        }
        public TView ActivateView<TView>(string id, object viewModel) where TView: IView
        {
            return (TView) ActivateView(id, viewModel);
        }
        public TView ActivateView<TView>(string id, int index, object viewModel) where TView: IView
        {
            return (TView) ActivateView(id, index, viewModel);
        }

        public TView[] ActivateViews<TView, T>(string id, IEnumerable<T> items) where TView: IView<T> where T: class
        {
            var ix = 0;
            return items.Select(x => ActivateView<TView>(id, ++ix, x)).ToArray();
        }

        public IRegion Clear()
        {
            foreach (var v in allViews.Keys)
            {
                DeactivateView(v);
            }
            foreach (var value in allViews.Values)
            {
                value.Dispose();
            }
            allViews.Clear();
            return this;
        }

        public bool DeactivateView(string id)
        {
            lock (syncRoot)
            {
                return DoDeactivateView(id);
            }
        }

        private bool DoDeactivateView(string id)
        {
            IView view;
            if (activeViews.TryGetValue(id, out view))
            {
                activeViews.Remove(id);
                var cch = ContentChange;
                if (cch != null)
                {
                    cch(this, view, RegionModificationType.ViewDeactivated);
                }
                view.Refreshed -= OnViewRefreshed;
                if (view is IViewInit)
                {
                    ((IViewInit) view).ReleaseEventBus();
                }
                logger.Debug("View '{0}' has been deactivated from region '{1}'", id, Path);
                return true;
            }
            return false;
        }

        public event Action<IRegion, IView, RegionModificationType> ContentChange;

        public string Name { get { return template.RegionName; } }
        public RegionLayout Layout { get { return template.Layout; } }
		public ViewBag ActiveViews { get { return activeViewsBag; } }
		public ViewBag AllViews { get { return allViewsBag; } }
        public IView OwnerView { get { return ownerView; } }
		public string Path { get; internal set; }
    }
}
