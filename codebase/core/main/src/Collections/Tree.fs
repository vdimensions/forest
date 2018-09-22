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
            | [] -> 
                let mutable ns = list.Empty
                for s in (List.rev st.Hierarchy.[parentID.Parent]) do
                    if not(s.Equals(parentID)) then ns <- s::ns
                let newHierarchy = 
                    st.Hierarchy 
                    |> Map.remove parentID.Parent 
                    |> Map.add parentID.Parent ns
                    |> Map.remove parentID
                ({ Hierarchy = newHierarchy }, parentID::lst)
            | head::_ -> 
                (st, lst) |> doRemove head |> doRemove parentID
        (tree, []) |> doRemove node 
        
    let filter (node:TreeNode) (matcher:(TreeNode -> bool)) (tree:Tree) : TreeNode list =
        let mutable matches = List.empty
        for child in getChildren node tree do
            if matcher(child) then
               matches <- child :: matches
        matches

    let tryFindView (node:TreeNode) (region:rname) (view:vname) (tree:Tree) : TreeNode option =
        let cmp = StringComparer.Ordinal
        tree |> getChildren node |> List.filter (fun x -> cmp.Equals(x.Region, region) ) |> List.tryFind (fun x -> cmp.Equals(x.View, view) )

    let root = { 
        Hierarchy = Map.empty.Add(TreeNode.shell, List.empty);
    }

