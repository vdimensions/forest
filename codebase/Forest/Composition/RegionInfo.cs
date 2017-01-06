using System;
using System.Collections.Generic;
using Forest.Collections;

namespace Forest.Composition
{
	public class RegionInfo
	{
		private readonly string name;
		private ViewMap activeViews;
		private ViewMap allViews;

		public RegionInfo (string name, ViewMap activeViews, ViewMap allViews)
		{
			this.name = name;
			this.activeViews = activeViews;
			this.allViews = allViews;
		}
		
		public string Name { get { return name; } }
		public ViewMap ActiveViews { get { return activeViews; } }
		public ViewMap AllViews { get { return allViews; } }
	}
}

