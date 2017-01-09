using System;
using System.Collections.Generic;

using Forest.Collections;


namespace Forest.Composition
{
	public sealed class RegionInfo
	{
		private readonly string name;
		private readonly ViewMap activeViews;
		private readonly ViewMap allViews;
		private readonly ViewInfo ownerView;

		internal RegionInfo(IRegion region) : this(region.Name, region.ActiveViews, region.AllViews, region.OwnerView) { }
		internal RegionInfo(string name, ViewMap activeViews, ViewMap allViews, IView ownerView)
		{
			this.name = name;
			this.activeViews = activeViews;
			this.allViews = allViews;
			this.ownerView = new ViewInfo(ownerView);
		}
		
		public string Name { get { return name; } }
		public ViewMap ActiveViews { get { return activeViews; } }
		public ViewMap AllViews { get { return allViews; } }
		public ViewInfo OwnerView { get { return ownerView; } }
	}
}
