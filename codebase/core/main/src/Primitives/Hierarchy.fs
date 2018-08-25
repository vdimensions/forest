namespace Forest

open Forest

open System
open System.Diagnostics


[<Serializable>]
[<DebuggerDisplay("{Hierarchy}")>]
type [<Struct>] internal Hierarchy = {
    [<DebuggerBrowsable(DebuggerBrowsableState.RootHidden)>]
    Hierarchy: Map<HierarchyKey, HierarchyKey list>;
}

[<RequireQualifiedAccess>]
module internal Hierarchy =
    
    let inline getChildren (id:HierarchyKey) (state:Hierarchy) : HierarchyKey list =
        match state.Hierarchy.TryFind id with
        | Some data -> data
        | None -> List.Empty

    let insert (id:HierarchyKey) (state:Hierarchy) : Hierarchy =
        match state.Hierarchy.TryFind id with
        | Some _ -> state // prevent multiple inserts
        | None ->
            let parent = id.Parent
            let list = 
                match state.Hierarchy.TryFind parent with
                | Some list -> list
                | None -> List.empty
            let h = state.Hierarchy.Remove(parent).Add(parent, list @ [id]).Add(id, List.empty)
            { Hierarchy = h }

    let remove (id:HierarchyKey) (state:Hierarchy) : Hierarchy*HierarchyKey list =
        let rec doRemove parentID (st, lst) =
            match st |> getChildren parentID  with
            | [] -> ({ Hierarchy = st.Hierarchy.Remove(parentID) }, [parentID] @ lst)
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

    let tryFindView (id: HierarchyKey) (regionName: string) (viewName: string) (state: Hierarchy) : HierarchyKey option =
        let cmp = StringComparer.Ordinal
        state |> getChildren id |> List.filter (fun x -> cmp.Equals(x.Region, regionName) ) |> List.tryFind (fun x -> cmp.Equals(x.View, viewName) )

    let empty = { 
        Hierarchy = Map.empty.Add(HierarchyKey.shell, List.Empty);
    }

