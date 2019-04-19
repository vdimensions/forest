namespace Forest.Templates

open Forest
open Forest.Templates.Raw


[<RequireQualifiedAccess>]
module TemplateCompiler =
    let rec private expandView (node : TreeNode) (vcl : ViewContents list) =
        let mutable result = [(ExecutionContext.Operation.InstantiateViewByNode(node, None))]
        for vc in vcl do
            match vc with
            | Region (name, contents) -> result <- expandRegion node name contents @ result
            | _ -> invalidOp ("Unexpected view content item " + vc.ToString())
        result
    and private expandRegion (parent : TreeNode) (region : rname) (rcl : RegionContents list) =
        let mutable result = List.empty
        for rc in rcl do
            match rc with
            | View (name, contents) ->
                let node = TreeNode.newKey region name parent
                result <- expandView node contents @ result
            | Placeholder _ -> 
                // Unconsumed placeholders are common for views that can themselves serve as a master
                // When compiling a final view, they are ignored as they serve no purpose
                () 
            | _ -> 
                invalidOp ("Unexpected region content item " + rc.ToString())
        result

    let internal compileOps (template : TemplateDefinition) =
        let (templateParentNode, templateRegion) = (TreeNode.shell, TreeNode.shell.Region)
        let templateNode = templateParentNode |> TreeNode.newKey templateRegion template.Name
        let ops = ExecutionContext.Operation.ClearRegion(templateParentNode, templateRegion)::(expandView templateNode template.Contents |> List.rev)
        ops

    [<CompiledName("Compile")>]
    let compile = compileOps >> ExecutionContext.Operation.Multiple