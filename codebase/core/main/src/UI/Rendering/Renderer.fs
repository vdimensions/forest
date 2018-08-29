namespace Forest.UI.Rendering

open Forest


type [<Struct>] RenderNode = {
    [<CompiledName("ID")>]
    id:sname;
    [<CompiledName("Model")>]
    model:obj;
    [<CompiledName("Region")>]
    regions:Map<rname, RenderNode list>;
    [<CompiledName("Commands")>]
    commands:Map<cname, ICommandModel>;
}

type [<Interface>] IForestRenderer =
    abstract member ProcessNode: RenderNode -> RenderNode

type ForestRenderer private(chainRender:(RenderNode -> RenderNode)) =
    /// Stores the rendered node state
    let mutable nodeMap:Map<string, RenderNode> = Map.empty
    /// Stores the original viewmodel state. Used to determine which nodes to be re-rendered
    let mutable modelMap:Map<string, obj> = Map.empty

    let createCommandModel (descriptor:ICommandDescriptor) : (cname*ICommandModel) =
        (descriptor.Name, upcast Command.Model(descriptor.Name))

    new (renderChain:IForestRenderer seq) = ForestRenderer(renderChain |> Seq.fold (fun acc f -> (acc >> f.ProcessNode)) (fun (r:RenderNode) -> r))

    interface IForestStateVisitor with
        member __.BFS key _ viewModel descriptor = 
            // go ahead top-to-bottom and collect the basic model data
            let hash = key.Hash
            let commands = descriptor.Commands |> Seq.map createCommandModel |> Map.ofSeq
            let node = { id=hash;model=viewModel;regions=Map.empty;commands=commands }
            nodeMap <- nodeMap |> Map.add node.id node
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
                        match parent.regions.TryFind region with
                        | Some nodes -> processedNode::nodes
                        | None -> List.singleton processedNode
                    let newRegions = parent.regions |> Map.remove region |> Map.add region newRegionContents
                    if canSkipRenderCall then nodeMap else nodeMap |> Map.remove key.Hash |> Map.add key.Hash processedNode
                    |> Map.remove parentKey
                    |> Map.add parentKey { parent with regions=newRegions }
                | (_, None, Some node) ->
                    let processedNode = chainRender node
                    nodeMap |> Map.remove key.Hash |> Map.add key.Hash processedNode
                | _ -> nodeMap
        member this.Complete() = 
            ()