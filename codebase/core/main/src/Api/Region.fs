namespace Forest


module Region =
    [<CompiledName("ActivateView")>]
    let activateByName (name : vname) (region : IRegion) : IView =
        region.ActivateView(name)

    [<CompiledName("ActivateView")>]
    let activateForModel<'m> (name : vname) (model : 'm) (region : IRegion) : IView<'m> =
        region.ActivateView(name, model)

    [<CompiledName("ActivateView")>]
    let activate<'V, 'M when 'V :> IView<'M>> (model : 'M) (region : IRegion) : 'V =
        region.ActivateView<'V, 'M>(model)

