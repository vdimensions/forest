using System;
using System.Collections.Generic;
using Forest.Collections;

namespace Forest.Composition
{
	
	public sealed class RegionMap : ReadOnlyDictionary<string, IRegion>
	{
		internal RegionMap(IDictionary<string, IRegion> dictionary) : base(dictionary) { }
	}
}
