using System.Collections.Generic;

using Axle.Forest.UI.Presentation;

namespace Axle.Forest.UI.Composition.Sdk
{
    public interface IViewState
    {
        IViewNode Current { get; set; }
    }
}
