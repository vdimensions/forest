using Forest.Commands;
using Forest.ComponentModel;

namespace Forest.Security
{
    public interface IForestSecurityManager
    {
        bool HasAccess(IForestViewDescriptor descriptor);
        bool HasAccess(IForestCommandDescriptor descriptor);
    }
}