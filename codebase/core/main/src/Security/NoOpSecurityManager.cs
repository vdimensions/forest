using Forest.ComponentModel;

namespace Forest.Security
{
    internal sealed class NoOpSecurityManager : ISecurityManager
    {
        bool ISecurityManager.HasAccess(ICommandDescriptor descriptor) => true;
        bool ISecurityManager.HasAccess(ILinkDescriptor descriptor) => true;
        bool ISecurityManager.HasAccess(IViewDescriptor descriptor) => true;
    }
}
