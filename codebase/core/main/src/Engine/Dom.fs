namespace Forest
open Forest
open Forest.ComponentModel
open Forest.UI
open System.Collections.Immutable
open System.Collections.Generic
open System

type [<Sealed;NoComparison>] internal ForestDomRenderer private(visit : (DomNode -> DomNode), complete : (DomNode list -> unit), ctx : IForestContext) =
    /// Stores the rendered node state
    let mutable nodeMap : Map<thash, DomNode> = Map.empty
    /// Stores the original view-model state. Used to determine which nodes to be re-rendered
    let mutable modelMap : Map<thash, obj> = Map.empty
    /// A list holding the tuples of a view's hash and a boolean telling whether the view should be refreshed
    let mutable changeStateList : (thash*bool) list = List.empty

    let createCommandModel (descriptor : ICommandDescriptor) : (cname*ICommandModel) =
        (descriptor.Name, upcast Command.Model(descriptor.Name))

    let createLinkModel (descriptor : ILinkDescriptor) : (string*ILinkModel) =
        (descriptor.Name, upcast Link.Model(descriptor.Name))

    new (renderChain : IDomProcessor seq, ctx : IForestContext) = 
        ForestDomRenderer(
            renderChain |> Seq.fold (fun acc f -> (acc >> f.ProcessNode)) id, 
            renderChain |> Seq.fold (fun _ f -> (fun l -> f.Complete(l) )) ignore, 
            ctx)

    interface IForestStateVisitor with
        member __.BFS treeNode i viewState descriptor = 
            // go ahead top-to-bottom and collect the basic model data
            let hash = treeNode.InstanceID
            if descriptor |> ctx.SecurityManager.HasAccess then
                let commands = 
                    descriptor.Commands.Values
                    |> Seq.filter (fun cmd -> viewState.DisabledCommands.Contains cmd.Name |> not)
                    |> Seq.filter ctx.SecurityManager.HasAccess 
                    |> Seq.map createCommandModel
                    |> Map.ofSeq
                let links = 
                    descriptor.Links.Values
                    |> Seq.filter (fun lnk -> viewState.DisabledLinks.Contains lnk.Name |> not)
                    |> Seq.filter ctx.SecurityManager.HasAccess
                    |> Seq.map createLinkModel
                    |> Map.ofSeq
                let canSkipRenderCall = 
                    match modelMap.TryFind hash with
                    // TODO: some system views require to be rendered
                    //| _ when descriptor.IsSystemView -> true
                    | Some m -> obj.Equals(m, viewState.Model)
                    | None ->
                        modelMap <- modelMap |> Map.add hash viewState.Model
                        false
                let node = DomNode(hash, i, descriptor.Name, treeNode.Region, viewState.Model, null, ImmutableDictionary<string, IEnumerable<DomNode>>.Empty, ImmutableDictionary.CreateRange(StringComparer.Ordinal, commands |> Seq.map id), ImmutableDictionary.CreateRange(StringComparer.Ordinal, links |> Seq.map id))
                nodeMap <- nodeMap |> Map.add hash node
                changeStateList <- (hash, canSkipRenderCall)::changeStateList
                
        member __.DFS treeNode _ _ _ = 
            // go backwards bottom-to-top and properly update the hierarchy
            nodeMap <-
            match (treeNode.Parent.InstanceID, nodeMap.TryFind treeNode.Parent.InstanceID, nodeMap.TryFind treeNode.InstanceID) with
            | (parentKey, Some parent, Some node) ->
                let region = treeNode.Region
                let newRegionContents = 
                    match parent.Regions.TryGetValue region with
                    | (true, nodes) -> node::Seq.toList nodes
                    | (false, _) -> List.singleton node
                let newRegions = parent.Regions.Remove(region).Add(region, newRegionContents)
                let newParent = DomNode(parent.InstanceID, parent.Index, parent.Name, parent.Region, parent.Model, parent.Parent, newRegions, parent.Commands, parent.Links)
                let newNode = DomNode(node.InstanceID, node.Index, node.Name, node.Region, node.Model, newParent, node.Regions, node.Commands, node.Links)
                nodeMap
                |> Map.remove parentKey |> Map.add parentKey newParent
                |> Map.remove node.InstanceID |> Map.add node.InstanceID newNode
            | _ -> nodeMap

        member __.Complete() = 
            let mutable nodes : DomNode list = List.empty
            for (h, skip) in changeStateList do
                nodes <- (if (not skip) then visit nodeMap.[h] else nodeMap.[h]) :: nodes
            complete(nodes)
            // clean-up the accumulated state.
            nodeMap <- Map.empty
            modelMap <- Map.empty
            changeStateList <- List.empty