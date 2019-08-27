﻿using Forest.ComponentModel;

namespace Forest.Security
{
    internal sealed class NoopSecurityManager : ISecurityManager
    {
        bool ISecurityManager.HasAccess(ICommandDescriptor descriptor) => true;
        bool ISecurityManager.HasAccess(ILinkDescriptor descriptor) => true;
        bool ISecurityManager.HasAccess(IViewDescriptor descriptor) => true;
    }
}
