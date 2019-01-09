namespace Forest.Web.WebSharper

open System.Collections.Generic
open Forest
open Forest.UI
open WebSharper.UI
open Axle.Web.AspNetCore.Session
open Microsoft.AspNetCore.Http


type [<Sealed>] internal WebSharperForestFacade(forestContext : IForestContext, renderer : IPhysicalViewRenderer<WebSharperPhysicalViewWrapper>) =
    inherit DefaultForestFacade<WebSharperPhysicalViewWrapper>(forestContext, renderer)
    member __.Renderer with get() = renderer

and [<Sealed;NoComparison;>] internal PerSessionWebSharperForestFacade(httpContextAccessor : IHttpContextAccessor) =
    inherit SessionScoped<WebSharperForestFacade>(httpContextAccessor)
    member private this.Facade with get() : IForestFacade = upcast this.Current
    interface INodeStateProvider with member this.Nodes with get() = (this.Current.Renderer :?> INodeStateProvider).Nodes
    interface IForestFacade with member this.LoadTree tree = this.Facade.LoadTree tree
    interface ICommandDispatcher with member this.ExecuteCommand c h a = this.Facade.ExecuteCommand c h a
    interface IMessageDispatcher with member this.SendMessage m = this.Facade.SendMessage m

and [<NoComparison;NoEquality>] WebSharperPhysicalViewWrapper internal (commandDispatcher, hash, allNodes : IDictionary<thash, Node>) =
    inherit AbstractPhysicalView(commandDispatcher, hash)
    let mutable regionMap : Map<rname, WebSharperPhysicalViewWrapper list> = Map.empty

    static member domNode2Node dn =
        { Hash = dn.Hash; Name = dn.Name; Model = dn.Model; Regions = dn.Regions |> Map.map (fun _ v -> v |> List.map (fun x -> x.Hash) |> Array.ofList ) |> Map.toArray }

    override __.Refresh node = 
        allNodes.[node.Hash] <- node |> WebSharperPhysicalViewWrapper.domNode2Node

    member __.Embed region pv =
        regionMap <- regionMap |> Map.add region (match regionMap.TryFind region with Some data -> pv::data | None -> List.singleton pv)
        pv

    override __.Dispose _ = 
        hash |> allNodes.Remove |> ignore

type [<Sealed;NoEquality;NoComparison>] internal WebSharperTopLevelPhysicalViewWrapper(commandDispatcher, hash, topLevelViews: List<WebSharperTopLevelPhysicalViewWrapper>, allNodes) =
    inherit WebSharperPhysicalViewWrapper(commandDispatcher, hash, allNodes)
    member private __.base_Dispose disposing = base.Dispose disposing
    override this.Dispose disposing = 
        topLevelViews.Remove this |> ignore
        this.base_Dispose disposing

type [<Sealed;NoEquality;NoComparison>] WebSharperPhysicalViewRenderer() =
    inherit AbstractPhysicalViewRenderer<WebSharperPhysicalViewWrapper>()
    let topLevelViews = List<WebSharperTopLevelPhysicalViewWrapper>()
    let allNodes = Dictionary<thash,Node>(System.StringComparer.Ordinal)

    override __.CreatePhysicalView commandDispatcher domNode = 
        let result = new WebSharperTopLevelPhysicalViewWrapper(commandDispatcher, domNode.Hash, topLevelViews, allNodes)
        topLevelViews.Add result
        upcast result

    override __.CreateNestedPhysicalView commandDispatcher parent domNode =
        new WebSharperPhysicalViewWrapper(commandDispatcher, domNode.Hash, allNodes)
        |> parent.Embed domNode.Region

    interface INodeStateProvider with
        member __.Nodes 
            with get() = allNodes.Values |> Array.ofSeq
