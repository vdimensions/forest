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


namespace Forest.Composition
{
    public class RegionEventSubscriber : IDisposable
    {
        private Action<IRegion, IView, RegionModificationType> onViewChanged;
        private readonly IView view;

        public RegionEventSubscriber(IView view, Action<IRegion, IView, RegionModificationType> onContentChange)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            if (onContentChange == null)
            {
                throw new ArgumentNullException("onContentChange");
            }
            this.view = view;
            this.onViewChanged = onContentChange;
            TraverseRegions(
                view,
                r =>
                {
                    r.ContentChange += onContentChange;
                    foreach (var x in r.ActiveViews)
                    {
                        x.Refreshed += OnViewRefreshed;
                    }
                });
            view.Refreshed += OnViewRefreshed;
        }

        void OnViewRefreshed(IView obj)
        {
            var c = onViewChanged;
            if (c != null)
            {
                c(null, obj, RegionModificationType.RefreshRequested);
            }
        }

        private static void TraverseRegions(IView view, Action<IRegion> action)
        {
            foreach (var region in view.Regions)
            {
                action(region);
                foreach (var v in region.ActiveViews)
                {
                    TraverseRegions(v, action);
                }
            }
        }

        protected virtual void Dispose(bool disposing) { }
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            TraverseRegions(
                view,
                r =>
                {
                    r.ContentChange -= onViewChanged;
                    foreach (var x in r.ActiveViews)
                    {
                        x.Refreshed -= OnViewRefreshed;
                    }
                });
            view.Refreshed -= OnViewRefreshed;
            onViewChanged = null;
        }
    }
}