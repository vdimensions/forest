namespace Forest

type internal ForestRenderer private(chainRender:(DomNode -> DomNode), ctx:IForestContext) =
    /// Stores the rendered node state
    let mutable nodeMap:Map<string, DomNode> = Map.empty
    /// Stores the original viewmodel state. Used to determine which nodes to be re-rendered
    let mutable modelMap:Map<string, obj> = Map.empty

    let createCommandModel (descriptor:ICommandDescriptor) : (cname*ICommandModel) =
        (descriptor.Name, upcast Command.Model(descriptor.Name))

    new (renderChain:IDomRenderer seq, ctx:IForestContext) = ForestRenderer(renderChain |> Seq.fold (fun acc f -> (acc >> f.ProcessNode)) (fun (r:DomNode) -> r), ctx)

    interface IForestStateVisitor with
        member __.BFS key i model descriptor = 
            // go ahead top-to-bottom and collect the basic model data
            if descriptor |> ctx.SecurityManager.HasAccess  then
                let commands = descriptor.Commands |> Seq.filter ctx.SecurityManager.HasAccess |> Seq.map createCommandModel |> Map.ofSeq
                let node = { Key=key.Hash;Name=key.View;Index=i;Model=model;Regions=Map.empty;Commands=commands }
                nodeMap <- nodeMap |> Map.add node.Key node
        member __.DFS key _ viewModel _ = 
            // go backwards bottom-to-top and properly update the hierarchy
            nodeMap <-
                match (key.Parent.Hash, nodeMap.TryFind key.Parent.Hash, nodeMap.TryFind key.Hash) with
                | (parentKey, Some parent, Some node) ->
                    let canSkipRenderCall = 
                        match modelMap.TryFind key.Hash with
                        | Some vm -> 
                            obj.Equals(vm, viewModel)
                        | None -> 
                            modelMap <- modelMap |> Map.add key.Hash viewModel
                            false
                    let processedNode = if canSkipRenderCall then node else chainRender node
                    let region = key.Region
                    let newRegionContents = 
                        match parent.Regions.TryFind region with
                        | Some nodes -> processedNode::nodes
                        | None -> List.singleton processedNode
                    let newRegions = parent.Regions |> Map.remove region |> Map.add region newRegionContents
                    if canSkipRenderCall then nodeMap else nodeMap |> Map.remove key.Hash |> Map.add key.Hash processedNode
                    |> Map.remove parentKey
                    |> Map.add parentKey { parent with Regions=newRegions }
                | (_, None, Some node) ->
                    let processedNode = chainRender node
                    nodeMap |> Map.remove key.Hash |> Map.add key.Hash processedNode
                | _ -> nodeMap
        member this.Complete() = 
            ()