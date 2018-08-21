namespace Forest

type [<Interface>] ISecurityManager =
    abstract member HasAccess: principal: string -> descriptor: IForestDescriptor -> bool