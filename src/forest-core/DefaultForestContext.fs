namespace Forest


[<AbstractClassAttribute>]
type AbstractForestContext(viewRegistry: IViewRegistry) as self =
    member this.Registry with get(): IViewRegistry = viewRegistry
    interface IForestContext with
        member this.Registry = self.Registry

type DefaultForestContext(viewRegistry: IViewRegistry) =
    inherit AbstractForestContext(viewRegistry)
