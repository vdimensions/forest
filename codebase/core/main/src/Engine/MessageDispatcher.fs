﻿namespace Forest

open Forest.NullHandling


module internal MessageDispatcher =
    [<Literal>]
    let Name = "MessageDispatcher"
    let private Key = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region Name
    type [<Sealed;View(Name);NoComparison>] View() = inherit AbstractView()
    let Show (runtime:ForestRuntime) : View = 
        match null2opt <| runtime.Context.ViewRegistry.GetDescriptor Name with
        | Some _ -> ()
        | None -> runtime.Context.ViewRegistry.Register typeof<View> |> ignore
        Key |> runtime.GetOrActivateView