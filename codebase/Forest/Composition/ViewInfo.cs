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
		private readonly string id;
		private readonly object viewModel;
		private readonly RegionBag regions;
		private readonly RegionInfo containingRegion;
		private readonly IView view;

		internal ViewInfo(IView view) : this(view.ID, view.ViewModel, view.Regions, view.ContainingRegion) 
		{
			this.view = view;
		}
		private ViewInfo(string id, object viewModel, RegionBag regions, RegionInfo containingRegion)
		{
			this.id = id;
			this.viewModel = viewModel;
			this.regions = regions;
			this.containingRegion = containingRegion;
		}

		public string ID { get { return this.id; } }
		public object ViewModel { get { return viewModel; } }
		public RegionBag Regions { get { return this.regions; } }
		public RegionInfo ContainingRegion { get { return this.containingRegion; } }
		internal IView View { get { return view; } }
	}
}
