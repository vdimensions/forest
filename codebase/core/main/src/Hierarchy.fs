namespace Forest

open Forest

open System


[<RequireQualifiedAccess>]
module internal Hierarchy =
    [<Serializable>]
    type [<Struct>] State = {
        Hierarchy: Map<Identifier, Identifier list>;
    }

    let getChildren (id: Identifier) (state: State) : Identifier list =
        match state.Hierarchy.TryFind id with
        | Some data -> data
        | None -> List.Empty


    let add (parent: Identifier) (name: string) (state: State) : State*Identifier =
        let newValue = parent |> Identifier.addNew name
        let list = 
            match state.Hierarchy.TryFind parent with
            | Some list -> list
            | None -> List.empty
        let h = state.Hierarchy.Remove(parent).Add(parent, list @ [newValue])
        ({ Hierarchy = h }, newValue)

    let insert (parent: Identifier) (guid: Guid) (name: string) (state: State) : State*Identifier =
        let newValue = parent |> Identifier.add guid name
        let list = 
            match state.Hierarchy.TryFind parent with
            | Some list -> list
            | None -> List.empty
        let h = state.Hierarchy.Remove(parent).Add(parent, list @ [newValue])
        ({ Hierarchy = h }, newValue)

    let remove (id: Identifier) (state: State) : State*Identifier list =
        let rec doRemove parentID (st, lst) =
            match st |> getChildren parentID  with
            | [] -> ({ Hierarchy = st.Hierarchy.Remove(parentID) }, [parentID] @ lst)
            | head::_ -> (st, lst) |> doRemove head |> doRemove parentID

        let (noContents, removedIDs) = doRemove id (state, [])
        
        match Identifier.parentOf id with
        | Some parentID ->
            let siblings = noContents |> getChildren parentID
            let updatedSiblings = siblings |> List.except [id]
            
            if (updatedSiblings.Length = siblings.Length)
            then (noContents, removedIDs)
            else ({ Hierarchy = noContents.Hierarchy.Remove(parentID).Add(parentID, updatedSiblings) }, removedIDs)
        | None -> (noContents, removedIDs)

    let tryFindRegion (id: Identifier) (regionName: string) (state: State) : Identifier option =
        match Identifier.isView id with
        | true -> state |> getChildren id |> List.tryFind (fun candidate -> StringComparer.Ordinal.Equals(Identifier.nameof candidate, regionName) )
        | false -> None

    let tryFindView (id: Identifier) (viewName: string) (state: State) : Identifier option =
        match Identifier.isRegion id with
        | true -> state |> getChildren id |> List.tryFind (fun candidate -> StringComparer.Ordinal.Equals(Identifier.nameof candidate, viewName) )
        | false -> None

    let empty = { 
        Hierarchy = Map.empty.Add(Identifier.shell, List.Empty);
    }

