using System.Collections.Generic;

using Axle.Forest.UI.Presentation;


namespace Axle.Forest.UI.Composition.Sdk
{
	public interface IViewComposition : IEnumerable<IRegionComposition>
	{
        IViewComposition Rebind(IViewNode view);

        IViewNode View { get; }

        IRegionComposition this[string regionName] { get; }
	}
}

