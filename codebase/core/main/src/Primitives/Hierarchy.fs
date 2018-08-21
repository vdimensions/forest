namespace Forest

open Forest

open System


[<Serializable>]
type [<Struct>] internal Hierarchy = {
    Hierarchy: Map<Identifier, Identifier list>;
}

[<RequireQualifiedAccess>]
module internal Hierarchy =
    
    let getChildren (id: Identifier) (state: Hierarchy) : Identifier list =
        match state.Hierarchy.TryFind id with
        | Some data -> data
        | None -> List.Empty

    let add (parent: Identifier) (region: string) (name: string) (state: Hierarchy) : Hierarchy*Identifier =
        let newValue = parent |> Identifier.addNew region name
        let list = 
            match state.Hierarchy.TryFind parent with
            | Some list -> list
            | None -> List.empty
        let h = state.Hierarchy.Remove(parent).Add(parent, list @ [newValue])
        ({ Hierarchy = h }, newValue)

    let insert (parent: Identifier) (guid: Guid) (region: string) (name: string) (state: Hierarchy) : Hierarchy*Identifier =
        let newValue = parent |> Identifier.add guid region name
        let list = 
            match state.Hierarchy.TryFind parent with
            | Some list -> list
            | None -> List.empty
        let h = state.Hierarchy.Remove(parent).Add(parent, list @ [newValue])
        ({ Hierarchy = h }, newValue)

    let remove (id: Identifier) (state: Hierarchy) : Hierarchy*Identifier list =
        let rec doRemove parentID (st, lst) =
            match st |> getChildren parentID  with
            | [] -> ({ Hierarchy = st.Hierarchy.Remove(parentID) }, [parentID] @ lst)
            | head::_ -> (st, lst) |> doRemove head |> doRemove parentID

        let (noContents, removedIDs) = doRemove id (state, [])
        
        match Identifier.isShell id.Parent with
        | false ->
            let parentID = id.Parent
            let siblings = noContents |> getChildren parentID
            let updatedSiblings = siblings |> List.except [id]
            
            if (updatedSiblings.Length = siblings.Length)
            then (noContents, removedIDs)
            else ({ Hierarchy = noContents.Hierarchy.Remove(parentID).Add(parentID, updatedSiblings) }, removedIDs)
        | true -> (noContents, removedIDs)

    let tryFindView (id: Identifier) (regionName: string) (viewName: string) (state: Hierarchy) : Identifier option =
        let cmp = StringComparer.Ordinal
        state |> getChildren id |> List.filter (fun x -> cmp.Equals(x.Region, regionName) ) |> List.tryFind (fun x -> cmp.Equals(x.Name, viewName) )

    let empty = { 
        Hierarchy = Map.empty.Add(Identifier.shell, List.Empty);
    }

