using System;
using Forest.StateManagement;
using Forest.UI;

namespace Forest
{
    [Obsolete]
    public interface IForestIntegrationProvider
    {
        [Obsolete]
        IPhysicalViewRenderer Renderer { get; }
        [Obsolete]
        IForestStateProvider StateProvider { get; }
    }
}