namespace Forest


type [<AbstractClass>] AbstractForestContext(viewRegistry: IViewRegistry) as self =
    //let mutable _domIndex: IDomIndex = IDomIndex
    member this.ViewRegistry with get(): IViewRegistry = viewRegistry
    interface IForestContext with
        member this.ViewRegistry = self.ViewRegistry

type DefaultForestContext(viewRegistry: IViewRegistry) =
    inherit AbstractForestContext(viewRegistry)
