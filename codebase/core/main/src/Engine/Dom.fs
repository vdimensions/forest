﻿namespace Forest

type [<Sealed>] internal ForestDomRenderer private(chainRender:(DomNode -> DomNode), complete:(unit -> unit), ctx:IForestContext) =
    /// Stores the rendered node state
    let mutable nodeMap:Map<string, DomNode> = Map.empty
    /// Stores the original viewmodel state. Used to determine which nodes to be re-rendered
    let mutable modelMap:Map<string, obj> = Map.empty

    let createCommandModel (descriptor:ICommandDescriptor) : (cname*ICommandModel) =
        (descriptor.Name, upcast Command.Model(descriptor.Name))

    new (renderChain:IDomProcessor seq, ctx:IForestContext) = 
        ForestDomRenderer(
            renderChain |> Seq.fold (fun acc f -> (acc >> f.ProcessNode)) (fun (r:DomNode) -> r), 
            renderChain |> Seq.fold (fun acc f -> (acc >> f.Complete)) ignore, 
            ctx)

    interface IForestStateVisitor with
        member __.BFS treeNode i model descriptor = 
            // go ahead top-to-bottom and collect the basic model data
            if descriptor |> ctx.SecurityManager.HasAccess then
                let commands = descriptor.Commands |> Seq.filter ctx.SecurityManager.HasAccess |> Seq.map createCommandModel |> Map.ofSeq
                let canSkipRenderCall = 
                    match modelMap.TryFind treeNode.Hash with
                    | None -> 
                        modelMap <- modelMap |> Map.add treeNode.Hash model
                        false
                    | Some m -> obj.Equals(m, model)
                let node = 
                    { Hash=treeNode.Hash; Name=treeNode.View; Index=i; Model=model; Regions=Map.empty; Commands=commands } 
                    |> (if canSkipRenderCall then id else chainRender)
                nodeMap <- nodeMap |> Map.add node.Hash node
                
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
                    |> Map.remove parentKey
                    |> Map.add parentKey { parent with Regions=newRegions }
                | _ -> nodeMap
        member __.Complete() = 
            complete()