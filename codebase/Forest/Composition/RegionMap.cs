using System;
using System.Collections;
using System.Collections.Generic;

using Forest.Collections;


namespace Forest.Composition
{
	
	public sealed class RegionMap : ReadOnlyDictionary<string, IRegion>, IEnumerable<IRegion>
	{
		internal RegionMap(IDictionary<string, IRegion> dictionary) : base(dictionary) { }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator (); }
		public IEnumerator<IRegion> GetEnumerator() { return Values.GetEnumerator (); }
	}
}
