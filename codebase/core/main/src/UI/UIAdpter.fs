namespace Forest.UI

open Forest


type internal NodeState =
    | NewNode of node:DomNode
    | UpdatedNode of node:DomNode

type [<AbstractClass>] AbstractUIAdapter<'A when 'A:> IViewAdapter>() =
    let mutable adapters:Map<hash, 'A> = Map.empty
    let mutable parentChildMap:Map<hash, hash*rname> = Map.empty
    let mutable nodesToDelete:Set<hash> = Set.empty
    let mutable nodeStates:List<NodeState> = List.empty

    abstract member CreateViewAdapter: key:hash * viewModel:obj -> 'A
    abstract member CreateNestedViewAdapter: key:hash * viewModel:obj * parentAdapter:'A * region:rname -> 'A

    interface IDomProcessor with
        member __.ProcessNode n =
            nodeStates <- 
                match adapters.TryFind n.Hash with
                | Some _ -> (UpdatedNode n)::nodeStates
                | None -> (NewNode n)::nodeStates
            nodesToDelete <- nodesToDelete |> Set.remove n.Hash
            for kvp in n.Regions do 
                for childNode in kvp.Value do
                    parentChildMap <- parentChildMap |> Map.add childNode.Hash (n.Hash, kvp.Key)
            n

        member this.Complete() = 
            for k in nodesToDelete do 
                match adapters.TryFind k with
                | Some v -> 
                    v.Dispose()
                    adapters <- adapters |> Map.remove k
                | None -> ()

            for nodeState in nodeStates do
                match nodeState with
                | NewNode n ->
                    match parentChildMap.TryFind n.Hash with
                    | Some (h, r) -> 
                        match adapters.TryFind h with
                        | Some a -> adapters <- adapters |> Map.add n.Hash (this.CreateNestedViewAdapter(n.Hash, n.Model, a, r))
                        | None -> failwithf "Could not locate view adapter %s that should parent %s" h n.Hash
                    | None -> adapters <- adapters |> Map.add n.Hash (this.CreateViewAdapter(n.Hash, n.Model))
                | UpdatedNode n ->
                    match adapters.TryFind n.Hash with
                    | Some a -> a.Update n.Model
                    | None -> failwithf "Could not locate view adapter for %s" n.Hash

            nodeStates <- List.empty
            parentChildMap <- Map.empty
            // Update the nodes to delete to include the entire tree. 
            // Each node that should be retained will be removed form the list during traversal
            nodesToDelete <- adapters |> Seq.map (fun a -> a.Key) |> Set.ofSeq
            ()
