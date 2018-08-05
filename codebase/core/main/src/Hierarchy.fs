namespace Forest

open Forest

open System
open System.Text


type RegionID = Root | Region of data: ViewID * string
 and [<Struct>] ViewID(rid: RegionID, index: int, name: string) =
    static member Separator: char = '/'
    static member IndexSuffix: char = '#'

    static member CreateRootView ix name = ViewID(RegionID.Root, ix, name)

    member __.RegionID with get() = rid
    member __.Index with get() = index
    member __.Name with get() = name

    override this.ToString() =
        let sb = 
            match this.RegionID with 
            | Root -> StringBuilder()
            | Region (viewID, regionName) -> StringBuilder(viewID.ToString()).Append(ViewID.Separator).Append(regionName)
        sb.Append(ViewID.Separator).Append(this.Index).Append(ViewID.IndexSuffix).Append(this.Name).ToString()

[<RequireQualifiedAccess>]
module internal Hierarchy =

    type Key = 
        | ViewKey of ViewID//*Guid 
        | RegionKey of RegionID//*Guid

    type Error =
        | InconsistentKeyMapping of Key * Key
        | DataLeakage
        | KeyAlreadyPreset of Key
        | KeyNotFound of Key
        | KeyParentNotFound of Key
        | GuidNotFound of Key
        | HierarchyEntryAbsent of Key * Guid
        | WrongKeyAtIndex of Key * int * Key

    type [<Struct>] State = {
        //[<Obsolete>]
        PrimaryMap: Map<Key, Guid>;
        ReverseMap: Map<Guid, Key>;
        Hierarchy: Map<Guid, Guid list>
    }

    let private _initialState: State = { 
        PrimaryMap = Map.empty.Add(RegionKey Root, Guid.Empty); 
        ReverseMap = Map.empty.Add(Guid.Empty, RegionKey Root); 
        Hierarchy = Map.empty.Add(Guid.Empty, List.Empty) 
    }

    let inline private _getParentKey (key: Key) : Key option =
        match key with
        | RegionKey regionID ->
            match regionID with
            | Root -> None
            | Region (vid, _) -> Some (ViewKey vid)
        | ViewKey vid -> Some (RegionKey vid.RegionID)

    let private _addEntry (guid: Guid) (key: Key) (state: State) : Result<State*Key, Error> =
        match state.PrimaryMap.TryFind key with
        | Some _ -> Error (KeyAlreadyPreset key)
        | None -> 
            match _getParentKey key with
            | Some k -> 
                match state.PrimaryMap.TryFind k with
                | Some parentGuid ->
                    match state.Hierarchy.TryFind parentGuid with
                    | Some guids ->
                        let index = (state.Hierarchy.[parentGuid].Length)
                        let h = (state.Hierarchy.Remove parentGuid).Add (parentGuid, guids @ [guid])
                        let properKey =
                            match key with
                            | ViewKey v when v.Index < 0 -> ViewKey (ViewID(v.RegionID, index, v.Name))
                            | _ -> key
                        let (pm, rm) = (state.PrimaryMap.Add(properKey, guid), state.ReverseMap.Add(guid, properKey))
                        if (pm.Count = rm.Count) 
                        then Ok ({ PrimaryMap = pm; ReverseMap = rm; Hierarchy = h }, properKey)
                        else Error (DataLeakage)
                    | None -> Error (HierarchyEntryAbsent(k, parentGuid))
                | None -> Error (GuidNotFound k)
            | None -> Error (KeyParentNotFound key)
            
    let rec private _removeEntriesFor (guid: Guid) (state: State*Guid list) : Result<State*Guid list, Error> =
        let (sd, gl) = state
        let childGuids = 
            match sd.Hierarchy.TryFind guid with
            | Some guids -> guids
            | None -> List.Empty
        match childGuids with
        | [] -> 
            let h = sd.Hierarchy.Remove guid
            let pm =
                match sd.ReverseMap.TryFind guid with
                | Some k -> sd.PrimaryMap.Remove k
                | None -> sd.PrimaryMap
            let rm = sd.ReverseMap.Remove guid
            Ok ({ PrimaryMap = pm; ReverseMap = rm; Hierarchy = h}, [guid] @ gl)
        | head::_ -> _removeEntriesFor head state

    let private _compensateIndexGap (key: Key) (state: State*Guid list) : Result<State*Guid list, Error> =
        match key with
        | ViewKey v ->
            let (sd, gl) = state 
            let parentKey = RegionKey v.RegionID
            match sd.PrimaryMap.TryFind parentKey with
            | Some parentGuid ->
                let allSiblingKeys = 
                    match sd.Hierarchy.TryFind parentGuid with
                    | Some guidList -> 
                        guidList 
                        |> List.map (sd.ReverseMap.TryFind) 
                        |> List.choose id
                        |> List.map (fun wk -> match wk with | ViewKey v -> Some v | _ -> None)
                        |> List.choose id
                    | None -> List.Empty
                if (allSiblingKeys.[v.Index - 1] = v)
                then
                    let mutable (pm, rm) = (sd.PrimaryMap, sd.ReverseMap)
                    for i in (v.Index) .. (allSiblingKeys.Length - 1) do
                        let oldKey = (allSiblingKeys.[i])
                        let (newKey, oldKeyGuid) = (ViewID (oldKey.RegionID, (oldKey.Index - 1), oldKey.Name), pm.[(ViewKey oldKey)])
                        pm <- (pm.Remove (ViewKey oldKey)).Add(ViewKey newKey, oldKeyGuid)
                        rm <- (rm.Remove oldKeyGuid).Add(oldKeyGuid, ViewKey newKey)
                    Ok ({ PrimaryMap = pm; ReverseMap = rm; Hierarchy = sd.Hierarchy }, gl)
                else Error (WrongKeyAtIndex (key, v.Index, ViewKey allSiblingKeys.[v.Index - 1]))
            | None -> Error (GuidNotFound parentKey)
        | RegionKey _ -> Ok state


    let add (key: Key) (state: State) : Result<State*Guid, Error> =
        let guid = Guid.NewGuid()
        match _addEntry guid key state with
        | Ok (hs, _) -> Ok (hs, guid)
        | Error e -> Error e

    let remove (key: Key) (state: State) : Result<State*Guid list, Error> =
        match state.PrimaryMap.TryFind key with
        | Some guid ->
            Ok (state, List.Empty)
            >>= _removeEntriesFor guid
            >>= _compensateIndexGap key
        | None -> Error (GuidNotFound(key))

    let getGuid (key: Key) (state: State) : Result<Guid, Error> =
        match state.PrimaryMap.TryFind key with
        | Some guid -> Ok guid
        | None -> Error (KeyNotFound key)

    let getViewGuid (viewID: ViewID) (state: State) : Result<Guid, Error> = getGuid (ViewKey viewID) state

    let getRegionGuid (regionID: RegionID) (state: State) : Result<Guid, Error> = getGuid (RegionKey regionID) state

    let getViewID (guid: Guid) (state: State) : ViewID option =
        match state.ReverseMap.TryFind guid with
        | Some key -> match key with | ViewKey vk -> Some vk | _ -> None
        | None -> None

    let getRegionID (guid: Guid) (state: State) : RegionID option =
        match state.ReverseMap.TryFind guid with
        | Some key -> match key with | RegionKey rk -> Some rk | _ -> None
        | None -> None

    let getChildren (guid: Guid) (state: State) : Guid list =
        match state.Hierarchy.TryFind guid with
        | Some guids -> guids
        | None -> List.Empty

    let empty() = _initialState

