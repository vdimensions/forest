namespace Forest

type [<Interface>] ISecurityManager =
    abstract member HasAccess: descriptor: IForestDescriptor -> bool