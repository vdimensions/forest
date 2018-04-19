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
namespace Forest.Composition
{
	public sealed class ViewInfo
	{
		private readonly string _id;
		private readonly object _viewModel;
		private readonly RegionBag _regions;
		private readonly RegionInfo _containingRegion;
		private readonly IView _view;

		internal ViewInfo(IView view) : this(view.ID, view.ViewModel, view.Regions, view.ContainingRegion) 
		{
			_view = view;
		}
		private ViewInfo(string id, object viewModel, RegionBag regions, RegionInfo containingRegion)
		{
			_id = id;
			_viewModel = viewModel;
			_regions = regions;
			_containingRegion = containingRegion;
		}

		public string ID => _id;
	    public object ViewModel => _viewModel;
	    public RegionBag Regions => _regions;
	    public RegionInfo ContainingRegion => _containingRegion;
	    internal IView View => _view;
	}
}
