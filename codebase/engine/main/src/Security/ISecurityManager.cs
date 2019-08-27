using Forest.ComponentModel;

namespace Forest.Security
{
    public interface ISecurityManager
    {
        bool HasAccess(IViewDescriptor descriptor);
        bool HasAccess(ICommandDescriptor descriptor);
        bool HasAccess(ILinkDescriptor descriptor);
    }
}