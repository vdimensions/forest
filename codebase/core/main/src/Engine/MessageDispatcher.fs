namespace Forest

open Forest.NullHandling


module internal MessageDispatcher =
    [<Literal>]
    let Name = "MessageDispatcher"
    let private Key = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region Name
    type [<View(Name);Sealed;NoComparison>] View() = inherit LogicalView()
    let Show (runtime : ForestRuntime) : View = 
        match null2vopt <| runtime.Context.ViewRegistry.GetDescriptor Name with
        | ValueSome _ -> ()
        | ValueNone -> runtime.Context.ViewRegistry.Register typeof<View> |> ignore
        Key |> runtime.GetOrActivateView