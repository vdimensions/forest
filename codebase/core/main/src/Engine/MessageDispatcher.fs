namespace Forest

open Forest.NullHandling


module internal MessageDispatcher =
    [<Literal>]
    let Name = "MessageDispatcher"
    let private Key = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region Name
    type [<Sealed>] ViewModel() = 
        override __.Equals _ = true
    [<View(Name)>]
    type [<Sealed>] View() =
        inherit AbstractView<ViewModel>() with
        override __.Load() = ()
    let Reg (ctx:IForestContext) = 
        match null2opt <| ctx.ViewRegistry.GetDescriptor Name with
        | Some _ -> ()
        | None -> ctx.ViewRegistry.Register typeof<View> |> ignore
    let Get (ms:ForestRuntime) : View = 
        Key |> ms.GetOrActivateView