namespace Forest.Templates

open Forest
open Forest.Templates


[<RequireQualifiedAccess>]
module TemplateCompiler =
    let rec private expandView (node : Tree.Node) (vcl : Template.ViewItem seq) =
        let mutable result = [(Runtime.Operation.InstantiateViewByNode(node, None))]
        for vc in vcl do
            match vc with
            | :? Template.ViewItem.Region as r -> result <- expandRegion node r.Name r.Contents @ result
            | _ -> invalidOp ("Unexpected view content item " + vc.ToString())
        result
    and private expandRegion (parent : Tree.Node) (region : rname) (rcl : Template.RegionItem seq) =
        let mutable result = List.empty
        for rc in rcl do
            match rc with
            | :? Template.RegionItem.View as v ->
                let node = Tree.Node.Create(region, v.Name, parent)
                result <- expandView node v.Contents @ result
            | :? Template.RegionItem.Placeholder as placeholder -> 
                // Unconsumed placeholders are common for views that can themselves serve as a master
                // When compiling a final view, they are ignored as they serve no purpose
                () 
            | _ -> 
                invalidOp ("Unexpected region content item " + rc.ToString())
        result

    let internal compileOps (template : Template.Definition) =
        let (templateParentNode, templateRegion) = (Tree.Node.Shell, Tree.Node.Shell.Region)
        let templateNode = Tree.Node.Create(templateRegion, template.Name, templateParentNode)
        let ops = Runtime.Operation.ClearRegion(templateParentNode, templateRegion)::List.rev(expandView templateNode template.Contents)
        ops

    [<CompiledName("Compile")>]
    let compile = compileOps >> Runtime.Operation.Multiple