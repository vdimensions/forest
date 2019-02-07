namespace Forest


module Region =
    [<CompiledName("ActivateView")>]
    let activateViewNamed<'m> (name : vname) (model : 'm option) (region : IRegion) : IView<'m> =
        match model with
        | Some m -> region.ActivateView(name, m)
        | None -> region.ActivateView(name) :?> IView<'m>

    [<CompiledName("ActivateView")>]
    let activateView<'V, 'M when 'V :> IView<'M>> (model : 'M) (region : IRegion) : 'V =
        region.ActivateView<'V, 'M>(model)

    [<CompiledName("ActivateView")>]
    let clear (region : IRegion) = region.Clear()

