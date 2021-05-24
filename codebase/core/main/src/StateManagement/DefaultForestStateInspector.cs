using Axle.Verification;
using Forest.ComponentModel;

namespace Forest.StateManagement
{
    internal sealed class DefaultForestStateInspector : IForestStateInspector
    {
        public IForestViewDescriptor GetViewDescriptor(ForestState state, string instanceId)
        {
            return TryGetViewDescriptor(state, instanceId, out var descriptor) ? descriptor : null;
        }
        public bool TryGetViewDescriptor(ForestState state, string instanceId, out IForestViewDescriptor descriptor)
        {
            state.VerifyArgument(nameof(state)).IsNotNull();
            instanceId.VerifyArgument(nameof(instanceId)).IsNotNull();
            if (state.LogicalViews.TryGetValue(instanceId, out var logicalView))
            {
                descriptor = logicalView.Item1.Descriptor;
                return true;
            }
            descriptor = null;
            return false;
        }
    }
}