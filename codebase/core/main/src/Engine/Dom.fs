namespace Forest

type [<Sealed;NoComparison>] internal ForestDomRenderer private(visit : (DomNode -> DomNode), complete : (DomNode list -> unit), ctx : IForestContext) =
    /// Stores the rendered node state
    let mutable nodeMap : Map<thash, DomNode> = Map.empty
    /// Stores the original view-model state. Used to determine which nodes to be re-rendered
    let mutable modelMap : Map<thash, obj> = Map.empty
    /// A list holding the tuples of a view's hash and a boolean telling whether the view should be refreshed
    let mutable changeStateList : List<thash*bool> = List.empty

    let createCommandModel (descriptor : ICommandDescriptor) : (cname*ICommandModel) =
        (descriptor.Name, upcast Command.Model(descriptor.Name))

    new (renderChain : IDomProcessor seq, ctx : IForestContext) = 
        ForestDomRenderer(
            renderChain |> Seq.fold (fun acc f -> (acc >> f.ProcessNode)) id, 
            renderChain |> Seq.fold (fun acc f -> (fun l -> acc(l); f.Complete(l);)) ignore, 
            ctx)

    interface IForestStateVisitor with
        member __.BFS treeNode i model descriptor = 
            // go ahead top-to-bottom and collect the basic model data
            let hash = treeNode.Hash
            if descriptor |> ctx.SecurityManager.HasAccess then
                let commands = descriptor.Commands |> Seq.filter ctx.SecurityManager.HasAccess |> Seq.map createCommandModel |> Map.ofSeq
                let canSkipRenderCall = 
                    match modelMap.TryFind hash with
                    // TODO: some system views require to be rendered
                    | _ when descriptor.IsSystemView -> true
                    | Some m -> obj.Equals(m, model)
                    | None ->
                        modelMap <- modelMap |> Map.add hash model
                        false
                let node = 
                    { 
                        Hash = hash 
                        Name = treeNode.View
                        Region = treeNode.Region
                        Index = i
                        Model = model
                        Parent = None
                        Regions = Map.empty
                        Commands = commands
                        // TODO:
                        Links = Map.empty
                    } 
                nodeMap <- nodeMap |> Map.add hash node
                changeStateList <- (hash, canSkipRenderCall)::changeStateList
                
        member __.DFS treeNode _ _ _ = 
            // go backwards bottom-to-top and properly update the hierarchy
            nodeMap <-
            match (treeNode.Parent.Hash, nodeMap.TryFind treeNode.Parent.Hash, nodeMap.TryFind treeNode.Hash) with
            | (parentKey, Some parent, Some node) ->
                let region = treeNode.Region
                let newRegionContents = 
                    match parent.Regions.TryFind region with
                    | Some nodes -> node::nodes
                    | None -> List.singleton node
                let newRegions = parent.Regions |> Map.remove region |> Map.add region newRegionContents
                nodeMap
                |> Map.remove parentKey |> Map.add parentKey { parent with Regions = newRegions }
                |> Map.remove node.Hash |> Map.add node.Hash { node with Parent = Some parent }
            | _ -> nodeMap

        member __.Complete() = 
            let mutable nodes : DomNode list = List.empty
            for (h, skip) in changeStateList do
                let mutable node = nodeMap.[h]
                if (not skip) then 
                    node <- visit node
                nodes <- node :: nodes
            complete(nodes)
            // clean the accumulated state up.
            nodeMap <- Map.empty
            modelMap <- Map.empty
            changeStateList <- List.empty