namespace Forest
open Forest.UI

module private NoOp =
    [<Literal>]
    let private DefaultErrorMessage = "Forest is not initialized"

    type [<Sealed;NoComparison;NoEquality>] private Renderer(message : string) =
        inherit AbstractPhysicalViewRenderer<IPhysicalView>()
        override __.CreateNestedPhysicalView _ _ _ = invalidOp message
        override __.CreatePhysicalView _ _ = invalidOp message

    type [<Sealed;NoComparison;NoEquality>] private Facade(ctx : IForestContext) =
        inherit DefaultForestFacade<IPhysicalView>(ctx, Renderer(DefaultErrorMessage))
        override __.LoadTree _ = invalidOp DefaultErrorMessage
        override __.SendMessage _ = invalidOp DefaultErrorMessage
        override __.ExecuteCommand _ _ _ = invalidOp DefaultErrorMessage

    type [<Sealed>] FacadeProvider(ctx : IForestContext) =
        let facade : IForestFacade = upcast Facade(ctx)
        interface IForestFacadeProvider with member __.ForestFacade with get() = facade


