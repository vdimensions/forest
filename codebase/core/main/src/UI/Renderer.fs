namespace Forest.UI

open System

open Forest


type [<NoComparison;StructuralEquality>] internal NodeState =
    | NewNode of node:DomNode
    | UpdatedNode of node:DomNode

type [<AbstractClass;NoComparison>] AbstractUIRenderer<'R when 'R:> IViewRenderer> =
    val mutable private adapters:Map<thash, 'R>
    val mutable private parentChildMap:Map<thash, thash*rname>
    /// Contains a list of nodes to be deleted upon traversal completion. They represent removed views.
    val mutable private nodesToDelete:thash list
    /// Contains a list of nodes to be retained as they represent present views.
    val mutable private nodesToPreserve:thash list
    val mutable private nodeStates:NodeState list

    new() = { adapters = Map.empty; parentChildMap = Map.empty; nodesToDelete = List.empty; nodesToPreserve = List.empty; nodeStates = List.empty }

    abstract member CreateViewRenderer: n:DomNode -> 'R
    abstract member CreateNestedViewRenderer: n:DomNode * parent:'R * region:rname -> 'R

    interface IDomProcessor with
        member this.ProcessNode n =
            this.nodesToPreserve <- n.Hash::this.nodesToPreserve
            // Message dispatcher is an internal component and must not be rendered
            if not(StringComparer.Ordinal.Equals(n.Name, MessageDispatcher.Name)) then
                this.nodeStates <- 
                    match this.adapters.TryFind n.Hash with
                    | Some _ -> (UpdatedNode n)::this.nodeStates
                    | None -> (NewNode n)::this.nodeStates
                for kvp in n.Regions do 
                    for childNode in kvp.Value do
                        this.parentChildMap <- this.parentChildMap |> Map.add childNode.Hash (n.Hash, kvp.Key)
            n

        member this.Complete() = 
            for k in this.nodesToDelete |> List.except this.nodesToPreserve do 
                match this.adapters.TryFind k with
                | Some v -> 
                    v.Dispose()
                    this.adapters <- this.adapters |> Map.remove k
                | None -> ()

            this.nodesToPreserve <- List.empty
            this.nodesToDelete <- List.empty
            
            for nodeState in this.nodeStates do
                match nodeState with
                | NewNode n ->
                    match this.parentChildMap.TryFind n.Hash with
                    | Some (h, r) -> 
                        match this.adapters.TryFind h with
                        | Some a -> this.adapters <- this.adapters |> Map.add n.Hash (this.CreateNestedViewRenderer(n, a, r))
                        | None -> invalidOp(String.Format("Could not locate view adapter {0} that should parent {1}", h, n.Hash))
                    | None -> this.adapters <- this.adapters |> Map.add n.Hash (this.CreateViewRenderer(n))
                    this.nodesToDelete <- n.Hash :: this.nodesToDelete
                | UpdatedNode n ->
                    match this.adapters.TryFind n.Hash with
                    | Some a -> a.Update n
                    | None -> invalidOp(String.Format("Could not locate view adapter for {0}", n.Hash))
                    this.nodesToDelete <- n.Hash :: this.nodesToDelete

            this.nodeStates <- List.empty
            this.parentChildMap <- Map.empty
            ()
