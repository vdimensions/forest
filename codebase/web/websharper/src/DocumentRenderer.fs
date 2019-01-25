namespace Forest.Web.WebSharper

open System.Collections.Generic
open Axle.Web.AspNetCore.Session
open Forest
open Forest.UI
open WebSharper.UI
open Microsoft.AspNetCore.Http


type [<Sealed>] internal WebSharperForestFacade(forestContext : IForestContext, renderer : IPhysicalViewRenderer<RemotingPhysicalView>) =
    inherit DefaultForestFacade<RemotingPhysicalView>(forestContext, renderer)
    member __.Renderer with get() = renderer

and [<Sealed;NoComparison>] internal PerSessionWebSharperForestFacade(httpContextAccessor : IHttpContextAccessor) =
    inherit SessionScoped<WebSharperForestFacade>(httpContextAccessor)
    member private this.Facade with get() : IForestFacade = upcast this.Current
    interface INodeStateProvider with 
        member this.AllNodes with get() = (this.Current.Renderer :?> INodeStateProvider).AllNodes
        member this.UpdatedNodes with get() = (this.Current.Renderer :?> INodeStateProvider).UpdatedNodes
    interface IForestFacade with member this.LoadTree tree = this.Facade.LoadTree tree
    interface ICommandDispatcher with member this.ExecuteCommand c h a = this.Facade.ExecuteCommand c h a
    interface IMessageDispatcher with member this.SendMessage m = this.Facade.SendMessage m

and [<NoComparison;NoEquality>] internal RemotingPhysicalView (commandDispatcher, hash, allNodes : IDictionary<thash, Node>) =
    inherit AbstractPhysicalView(commandDispatcher, hash)
    let mutable regionMap : Map<rname, RemotingPhysicalView list> = Map.empty

    static member domNode2Node (dn : DomNode) =
        { Hash = dn.Hash; Name = dn.Name; Model = dn.Model; Regions = dn.Regions |> Map.map (fun _ v -> v |> List.map (fun x -> x.Hash) |> Array.ofList ) |> Map.toArray }

    override __.Refresh node = 
        allNodes.[node.Hash] <- node |> RemotingPhysicalView.domNode2Node

    member __.Embed region pv =
        regionMap <- regionMap |> Map.add region (match regionMap.TryFind region with Some data -> pv::data | None -> List.singleton pv)
        pv

    override __.Dispose _ = 
        hash |> allNodes.Remove |> ignore

type [<Sealed;NoEquality;NoComparison>] internal RemotingRootPhysicalView(commandDispatcher, hash, topLevelViews: List<RemotingRootPhysicalView>, allNodes) =
    inherit RemotingPhysicalView(commandDispatcher, hash, allNodes)
    member private __.base_Dispose disposing = base.Dispose disposing
    override this.Dispose disposing = 
        topLevelViews.Remove this |> ignore
        this.base_Dispose disposing

type [<Sealed;NoEquality;NoComparison>] internal WebSharperPhysicalViewRenderer() =
    inherit AbstractPhysicalViewRenderer<RemotingPhysicalView>()
    let topLevelViews = List<RemotingRootPhysicalView>()
    let allNodes = Dictionary<thash,Node>(System.StringComparer.Ordinal)

    override __.CreatePhysicalView commandDispatcher domNode = 
        let result = new RemotingRootPhysicalView(commandDispatcher, domNode.Hash, topLevelViews, allNodes)
        topLevelViews.Add result
        upcast result

    override __.CreateNestedPhysicalView commandDispatcher parent domNode =
        new RemotingPhysicalView(commandDispatcher, domNode.Hash, allNodes)
        |> parent.Embed domNode.Region

    interface INodeStateProvider with
        member __.AllNodes 
            with get() = allNodes.Values |> Array.ofSeq
        member __.UpdatedNodes 
            with get() = allNodes.Values |> Array.ofSeq
