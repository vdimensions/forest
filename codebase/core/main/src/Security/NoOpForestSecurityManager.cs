using Forest.Commands;
using Forest.ComponentModel;

namespace Forest.Security
{
    internal sealed class NoOpForestSecurityManager : IForestSecurityManager
    {
        bool IForestSecurityManager.HasAccess(IForestCommandDescriptor descriptor) => true;
        bool IForestSecurityManager.HasAccess(IForestViewDescriptor descriptor) => true;
    }
}
