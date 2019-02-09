namespace Forest
open System


module Region =
    [<CompiledName("ActivateView")>]
    let activateViewByName<'m> (name : vname) (model : obj option) (region : IRegion) : IView=
        match model with
        | Some m -> region.ActivateView(name, m)
        | None -> region.ActivateView(name)

    [<CompiledName("ActivateView")>]
    let activateViewByType (viewType : Type) (model : obj option) (region : IRegion) : IView =
        match model with
        | Some m -> region.ActivateView(viewType, m)
        | None -> region.ActivateView(viewType)

    [<CompiledName("ActivateView")>]
    let activateView<'V, 'M when 'V :> IView<'M>> (model : 'M) (region : IRegion) : 'V =
        region.ActivateView<'V, 'M>(model)

    [<CompiledName("Clear")>]
    let clear (region : IRegion) = region.Clear()

