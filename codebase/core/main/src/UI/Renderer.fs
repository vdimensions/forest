﻿namespace Forest.UI

open Forest


type [<NoComparison;StructuralEquality>] internal NodeState =
    | NewNode of node:DomNode
    | UpdatedNode of node:DomNode

type [<AbstractClass;NoComparison>] AbstractUIRenderer<'A when 'A:> IViewRenderer> =
    val mutable private adapters:Map<thash, 'A>
    val mutable private parentChildMap:Map<thash, thash*rname>
    val mutable private nodesToDelete:Set<thash>
    val mutable private nodeStates:List<NodeState>

    new() = { adapters = Map.empty; parentChildMap = Map.empty; nodesToDelete = Set.empty; nodeStates = List.empty }

    abstract member CreateViewAdapter: n:DomNode -> 'A
    abstract member CreateNestedViewAdapter: n:DomNode * parentAdapter:'A * region:rname -> 'A

    interface IDomProcessor with
        member this.ProcessNode n =
            this.nodeStates <- 
                match this.adapters.TryFind n.Hash with
                | Some _ -> (UpdatedNode n)::this.nodeStates
                | None -> (NewNode n)::this.nodeStates
            this.nodesToDelete <- this.nodesToDelete |> Set.remove n.Hash
            for kvp in n.Regions do 
                for childNode in kvp.Value do
                    this.parentChildMap <- this.parentChildMap |> Map.add childNode.Hash (n.Hash, kvp.Key)
            n

        member this.Complete() = 
            for k in this.nodesToDelete do 
                match this.adapters.TryFind k with
                | Some v -> 
                    v.Dispose()
                    this.adapters <- this.adapters |> Map.remove k
                | None -> ()

            for nodeState in this.nodeStates do
                match nodeState with
                | NewNode n ->
                    match this.parentChildMap.TryFind n.Hash with
                    | Some (h, r) -> 
                        match this.adapters.TryFind h with
                        | Some a -> this.adapters <- this.adapters |> Map.add n.Hash (this.CreateNestedViewAdapter(n, a, r))
                        | None -> failwithf "Could not locate view adapter %s that should parent %s" h n.Hash
                    | None -> this.adapters <- this.adapters |> Map.add n.Hash (this.CreateViewAdapter(n))
                | UpdatedNode n ->
                    match this.adapters.TryFind n.Hash with
                    | Some a -> a.Update n.Model
                    | None -> failwithf "Could not locate view adapter for %s" n.Hash

            this.nodeStates <- List.empty
            this.parentChildMap <- Map.empty
            // Update the nodes to delete to include the entire tree. 
            // Each node that should be retained will be removed form the list during traversal
            this.nodesToDelete <- this.adapters |> Seq.map (fun a -> a.Key) |> Set.ofSeq
            ()
