namespace Forest.Security

open Forest


type [<Sealed;NoComparison>] NoopSecurityManager() =
    interface ISecurityManager with
        member __.HasAccess(_ : ICommandDescriptor) = true
        member __.HasAccess(_ : IViewDescriptor) = true