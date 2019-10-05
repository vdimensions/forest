using Forest.StateManagement;
using Forest.UI;

namespace Forest
{
    public interface IForestIntegrationProvider
    {
        IPhysicalViewRenderer Renderer { get; }
        IForestStateProvider StateProvider { get; }
    }
}