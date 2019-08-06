namespace Forest.Security

open Forest
open Forest.ComponentModel


type [<Sealed;NoComparison>] NoopSecurityManager() =
    interface ISecurityManager with
        member __.HasAccess(_ : ICommandDescriptor) = true
        member __.HasAccess(_ : ILinkDescriptor) = true
        member __.HasAccess(_ : IViewDescriptor) = true
