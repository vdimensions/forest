namespace Forest.Collections

open Forest

open System
open System.Diagnostics


#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
[<DebuggerDisplay("{Hierarchy}")>]
type [<Struct;NoComparison;StructuralEquality>] internal Tree = {
    [<DebuggerBrowsable(DebuggerBrowsableState.RootHidden)>]
    Hierarchy: Map<TreeNode, TreeNode list>;
}

[<RequireQualifiedAccess>]
module internal Tree =
    let inline private getChildren (node:TreeNode) (tree:Tree) : TreeNode list =
        match tree.Hierarchy.TryFind node with
        | Some data -> data
        | None -> List.empty

    let insert (node:TreeNode) (tree:Tree) : Tree =
        match tree.Hierarchy.TryFind node with
        | Some _ -> tree // prevent multiple inserts
        | None ->
            let parent = node.Parent
            let list = 
                match tree.Hierarchy.TryFind parent with
                | Some list -> list
                | None -> List.empty
            let h = 
                tree.Hierarchy
                |> Map.remove parent 
                |> Map.add parent (node::list)
                |> Map.add node List.empty
            { Hierarchy = h }

    let remove (node:TreeNode) (tree:Tree) : Tree*TreeNode list =
        let rec doRemove parentID (st, lst) =
            match st |> getChildren parentID  with
            | [] -> ({ Hierarchy = st.Hierarchy.Remove(parentID) }, parentID::lst)
            | head::_ -> (st, lst) |> doRemove head |> doRemove parentID

        let (noContents, removedNodes) = doRemove node (tree, [])
        
        match TreeNode.isShell node.Parent with
        | false ->
            let parentNode = node.Parent
            let siblings = noContents |> getChildren parentNode
            let updatedSiblings = siblings |> List.except [node]
            
            if (updatedSiblings.Length = siblings.Length)
            then (noContents, removedNodes)
            else ({ Hierarchy = noContents.Hierarchy.Remove(parentNode).Add(parentNode, updatedSiblings) }, removedNodes)
        | true -> (noContents, removedNodes)

    let tryFindView (node:TreeNode) (region:rname) (view:vname) (tree:Tree) : TreeNode option =
        let cmp = StringComparer.Ordinal
        tree |> getChildren node |> List.filter (fun x -> cmp.Equals(x.Region, region) ) |> List.tryFind (fun x -> cmp.Equals(x.View, view) )

    let root = { 
        Hierarchy = Map.empty.Add(TreeNode.shell, List.empty);
    }

