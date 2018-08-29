namespace Forest.Collections

open Forest

open System
open System.Diagnostics


[<Serializable>]
[<DebuggerDisplay("{Hierarchy}")>]
type [<Struct>] internal Tree = {
    [<DebuggerBrowsable(DebuggerBrowsableState.RootHidden)>]
    Hierarchy: Map<HierarchyKey, HierarchyKey list>;
}

[<RequireQualifiedAccess>]
module internal Tree =
    let inline getChildren (id:HierarchyKey) (hierarchy:Tree) : HierarchyKey list =
        match hierarchy.Hierarchy.TryFind id with
        | Some data -> data
        | None -> List.empty

    let insert (id:HierarchyKey) (hierarchy:Tree) : Tree =
        match hierarchy.Hierarchy.TryFind id with
        | Some _ -> hierarchy // prevent multiple inserts
        | None ->
            let parent = id.Parent
            let list = 
                match hierarchy.Hierarchy.TryFind parent with
                | Some list -> list
                | None -> List.empty
            let h = 
                hierarchy.Hierarchy
                |> Map.remove parent 
                |> Map.add parent (id::list)
                |> Map.add id List.empty
            { Hierarchy = h }

    let remove (id:HierarchyKey) (state:Tree) : Tree*HierarchyKey list =
        let rec doRemove parentID (st, lst) =
            match st |> getChildren parentID  with
            | [] -> ({ Hierarchy = st.Hierarchy.Remove(parentID) }, parentID::lst)
            | head::_ -> (st, lst) |> doRemove head |> doRemove parentID

        let (noContents, removedIDs) = doRemove id (state, [])
        
        match HierarchyKey.isShell id.Parent with
        | false ->
            let parentID = id.Parent
            let siblings = noContents |> getChildren parentID
            let updatedSiblings = siblings |> List.except [id]
            
            if (updatedSiblings.Length = siblings.Length)
            then (noContents, removedIDs)
            else ({ Hierarchy = noContents.Hierarchy.Remove(parentID).Add(parentID, updatedSiblings) }, removedIDs)
        | true -> (noContents, removedIDs)

    let tryFindView (id:HierarchyKey) (regionName:rname) (viewName:vname) (state:Tree) : HierarchyKey option =
        let cmp = StringComparer.Ordinal
        state |> getChildren id |> List.filter (fun x -> cmp.Equals(x.Region, regionName) ) |> List.tryFind (fun x -> cmp.Equals(x.View, viewName) )

    let empty = { 
        Hierarchy = Map.empty.Add(HierarchyKey.shell, List.empty);
    }

