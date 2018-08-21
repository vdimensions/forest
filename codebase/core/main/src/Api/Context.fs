namespace Forest

open System


type [<Interface>] IForestContext =
    abstract ViewRegistry: IViewRegistry with get
    // TODO: renderers
    // TODO: security