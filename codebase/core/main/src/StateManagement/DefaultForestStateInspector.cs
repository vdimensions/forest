using Axle.Verification;
using Forest.ComponentModel;

namespace Forest.StateManagement
{
    internal sealed class DefaultForestStateInspector : IForestStateInspector
    {
        public IViewDescriptor GetViewDescriptor(ForestState state, string instanceId)
        {
            return TryGetViewDescriptor(state, instanceId, out var descriptor) ? descriptor : null;
        }
        public bool TryGetViewDescriptor(ForestState state, string instanceId, out IViewDescriptor descriptor)
        {
            state.VerifyArgument(nameof(state)).IsNotNull();
            instanceId.VerifyArgument(nameof(instanceId)).IsNotNull();
            if (state.LogicalViews.TryGetValue(instanceId, out var logicalView))
            {
                descriptor = logicalView.Descriptor;
                return true;
            }
            descriptor = null;
            return false;
        }
    }
}