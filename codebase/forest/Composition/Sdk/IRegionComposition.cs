using Axle.Forest.UI.Presentation;

namespace Axle.Forest.UI.Composition.Sdk
{	
	public interface IRegionComposition
	{
        IRegionNode Region { get; }

        IViewComposition this[string regionName] { get; set; }
	}
}
