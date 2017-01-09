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
	public sealed class RegionInfo
	{
		private readonly string name;
		private readonly ViewBag activeViews;
		private readonly ViewBag allViews;
		private readonly ViewInfo ownerView;

		internal RegionInfo(IRegion region) : this(region.Name, region.ActiveViews, region.AllViews, region.OwnerView) { }
	    private RegionInfo(string name, ViewBag activeViews, ViewBag allViews, IView ownerView)
		{
			this.name = name;
			this.activeViews = activeViews;
			this.allViews = allViews;
			this.ownerView = new ViewInfo(ownerView);
		}
		
		public string Name { get { return name; } }
		public ViewBag ActiveViews { get { return activeViews; } }
		public ViewBag AllViews { get { return allViews; } }
		public ViewInfo OwnerView { get { return ownerView; } }
	}
}
