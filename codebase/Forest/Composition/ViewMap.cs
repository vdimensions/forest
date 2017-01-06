using System;
using System.Collections.Generic;
using Forest.Collections;

namespace Forest.Composition
{
	public sealed class ViewMap : ReadOnlyDictionary<string, IView>
	{
		internal ViewMap(IDictionary<string, IView> dictionary) : base(dictionary) { }
	}
}
