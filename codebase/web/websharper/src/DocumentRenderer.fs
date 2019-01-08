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
    interface IForestFacade with member this.LoadTemplate template = this.Facade.LoadTemplate template
    interface ICommandDispatcher with member this.ExecuteCommand h c a = this.Facade.ExecuteCommand h c a
    interface IMessageDispatcher with member this.SendMessage m = this.Facade.SendMessage m

and [<NoComparison;NoEquality>] WebSharperPhysicalViewWrapper internal (commandDispatcher, hash, allNodes : IDictionary<thash, Node>, registry : IWebSharperTemplateRegistry) =
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

    member __.Doc (rdata) : Doc =
        let node = allNodes.[hash]
        let x = node.Name |> registry.Get 
        x.Doc rdata node

type [<Sealed;NoEquality;NoComparison>] internal WebSharperTopLevelPhysicalViewWrapper(commandDispatcher, hash, topLevelViews: List<WebSharperTopLevelPhysicalViewWrapper>, allNodes, registry) =
    inherit WebSharperPhysicalViewWrapper(commandDispatcher, hash, allNodes, registry)
    member private __.base_Dispose disposing = base.Dispose disposing
    override this.Dispose disposing = 
        topLevelViews.Remove this |> ignore
        this.base_Dispose disposing

type [<Sealed;NoEquality;NoComparison>] WebSharperPhysicalViewRenderer(registry : IWebSharperTemplateRegistry) =
    inherit AbstractPhysicalViewRenderer<WebSharperPhysicalViewWrapper>()
    let topLevelViews = List<WebSharperTopLevelPhysicalViewWrapper>()
    let allNodes = Dictionary<thash,Node>(System.StringComparer.Ordinal)

    override __.CreatePhysicalView commandDispatcher domNode = 
        let hash = domNode.Hash
        let result = new WebSharperTopLevelPhysicalViewWrapper(commandDispatcher, hash, topLevelViews, allNodes, registry)
        topLevelViews.Add result
        upcast result

    override __.CreateNestedPhysicalView commandDispatcher parent domNode =
        new WebSharperPhysicalViewWrapper(commandDispatcher, domNode.Hash, allNodes, registry)
        |> parent.Embed domNode.Region

    interface INodeStateProvider with
        member __.Nodes 
            with get() = allNodes.Values |> Array.ofSeq
