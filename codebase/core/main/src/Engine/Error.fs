namespace Forest

open Forest.NullHandling


module internal Error =
    [<Literal>]
    let Name = "Error"
    let private Key = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region Name
    type ViewModel() = class end
    [<View(Name)>]
    type View() =
        inherit AbstractView<ViewModel>(ViewModel()) with
        override __.Load() = ()
    let Reg (ctx:IForestContext) = 
        match null2opt <| ctx.ViewRegistry.GetDescriptor Name with
        | Some _ -> ()
        | None -> ctx.ViewRegistry.Register typeof<View> |> ignore
    let Show (ms:ForestRuntime) : View = 
        Key |> ms.GetOrActivateView