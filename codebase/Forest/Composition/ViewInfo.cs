using System;
using System.Collections.Generic;

using Forest.Collections;


namespace Forest.Composition
{
	public sealed class ViewInfo
	{
		private readonly string id;
		private readonly object viewModel;
		private readonly RegionMap regions;
		private readonly RegionInfo containingRegion;
		private readonly IView view;

		internal ViewInfo(IView view) : this(view.ID, view.ViewModel, view.Regions, view.ContainingRegion) 
		{
			this.view = view;
		}
		private ViewInfo(string id, object viewModel, RegionMap regions, RegionInfo containingRegion)
		{
			this.id = id;
			this.viewModel = viewModel;
			this.regions = regions;
			this.containingRegion = containingRegion;
		}

		public string ID { get { return this.id; } }
		public object ViewModel { get { return viewModel; } }
		public RegionMap Regions { get { return this.regions; } }
		public RegionInfo ContainingRegion { get { return this.containingRegion; } }
		internal IView View { get { return view; } }
	}
}
