namespace Forest
open Axle.Option


module internal Error =
    [<Literal>]
    let Name = "Error"
    let private Key = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region Name
    type [<Sealed;NoComparison>] Model() = class end
    type [<Sealed;View(Name);NoComparison>] View() =
        inherit LogicalView<Model>(Model()) with
        override __.Load() = ()
    let Reg (ctx : IForestContext) = 
        match null2opt <| ctx.ViewRegistry.GetDescriptor Name with
        | Some _ -> ()
        | None -> ctx.ViewRegistry.Register typeof<View> |> ignore
    let Show (runtime : ForestExecutionContext) : View = 
        Key |> runtime.GetOrActivateView