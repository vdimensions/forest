namespace Forest


type [<AbstractClass>] AbstractForestContext(viewRegistry: IViewRegistry) as self =
    member __.ViewRegistry with get(): IViewRegistry = viewRegistry
    interface IForestContext with
        member __.ViewRegistry = self.ViewRegistry

type DefaultForestContext(viewRegistry: IViewRegistry) =
    inherit AbstractForestContext(viewRegistry)
