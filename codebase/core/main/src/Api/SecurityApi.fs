namespace Forest.Security

open Forest

type [<Interface>] ISecurityManager =
    abstract member HasAccess: descriptor : IViewDescriptor -> bool
    abstract member HasAccess: descriptor : ICommandDescriptor -> bool