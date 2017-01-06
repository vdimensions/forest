using System;
using Forest.Composition;

namespace Forest
{
	public class RegionInfo
	{
		private readonly string name;
		private ViewMap views;

		public RegionInfo (string name)
		{
			this.name = name;
		}
		
		public string Name { get { return name; } }
		public ViewMap Views { get { return views; } }
	}
}

