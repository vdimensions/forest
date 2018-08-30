namespace Forest.Templates

open Forest
open Forest.Templates.Raw


[<RequireQualifiedAccess>]
module TemplateCompiler =
    let rec private expandView (node:TreeNode) (vcl:ViewContents list) =
        let mutable result = [(Runtime.Operation.InstantiateView node)]
        for vc in vcl do
            match vc with
            | Region (name, contents) ->
                for op in expandRegion node name contents do result <- op::result
        result
    and private expandRegion (parent:TreeNode) (region:rname) (rcl:RegionContents list) =
        let mutable result = List.empty
        for rc in rcl do
            match rc with
            | View (name, contents) ->
                let node = TreeNode.newKey region name parent
                for op in expandView node contents do result <- op::result
        result
    [<CompiledName("Compile")>]
    let compile (template:TemplateDefinition) =
        let templateNode = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region template.name
        let ops = expandView templateNode template.contents |> List.rev
        Runtime.Operation.Multiple ops