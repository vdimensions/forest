using Forest.ComponentModel;

namespace Forest.StateManagement
{
    public interface IForestStateInspector
    {
        IViewDescriptor GetViewDescriptor(ForestState state, string instanceId);
        bool TryGetViewDescriptor(ForestState state, string instanceId, out IViewDescriptor descriptor);
    }
}