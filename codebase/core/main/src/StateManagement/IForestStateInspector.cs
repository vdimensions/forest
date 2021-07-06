using Forest.ComponentModel;

namespace Forest.StateManagement
{
    public interface IForestStateInspector
    {
        IForestViewDescriptor GetViewDescriptor(ForestState state, string instanceId);
        bool TryGetViewDescriptor(ForestState state, string instanceId, out IForestViewDescriptor descriptor);
    }
}