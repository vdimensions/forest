namespace Forest
open Forest.UI

module private NoOp =
    [<Literal>]
    let private DefaultErrorMessage = "Forest is not initialized"

    type [<Sealed;NoComparison;NoEquality>] PhysicalViewRenderer private (message : string) =
        new () = PhysicalViewRenderer(DefaultErrorMessage)
        interface IPhysicalViewRenderer with
            member  __.CreateNestedPhysicalView _ _ _ = invalidOp message
            member  __.CreatePhysicalView _ _ = invalidOp message
//
//    type [<Sealed;NoComparison;NoEquality>] private Facade(ctx : IForestContext) =
//        interface ICommandDispatcher with
//            member __.ExecuteCommand _ _ _ = invalidOp DefaultErrorMessage
//        interface IMessageDispatcher with
//            member __.SendMessage _ = invalidOp DefaultErrorMessage
//        interface IForestFacade with
//            member __.LoadTree (_) = invalidOp DefaultErrorMessage
//            member __.LoadTree (_, _) = invalidOp DefaultErrorMessage
//            member __.RegisterSystemView<'sv when 'sv :> ISystemView> () = invalidOp DefaultErrorMessage
//            member __.Render<'pv when 'pv :> IPhysicalView> (_ : IPhysicalViewRenderer<'pv>) _ = invalidOp DefaultErrorMessage
//
//    type [<Sealed>] FacadeProvider(ctx : IForestContext) =
//        let facade : IForestFacade = upcast Facade(ctx)
//        interface IForestFacadeProvider with member __.ForestFacade with get() = facade