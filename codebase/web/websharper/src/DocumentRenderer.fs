namespace Forest.Web.WebSharper

open System.Collections.Generic
open Microsoft.AspNetCore.Http
open Axle.Web.AspNetCore.Session
open Forest
open Forest.UI
open WebSharper.UI


type [<Interface>] IDocumentStateProvider = 
    interface 
    // TODO
    end

type [<AbstractClass;NoComparison>] internal WebSharperForestFacadeProxy private (facade : IForestFacade, renderer : IPhysicalViewRenderer<RemotingPhysicalView>) =
    inherit ForestFacadeProxy(facade)
    new(forestContext : IForestContext, renderer : IPhysicalViewRenderer<RemotingPhysicalView>) = WebSharperForestFacadeProxy(DefaultForestFacade<RemotingPhysicalView>(forestContext, renderer), renderer)
    member __.Renderer with get() = renderer
            
and [<Sealed;NoComparison>] internal PerSessionWebSharperForestFacade(httpContextAccessor : IHttpContextAccessor) =
    inherit SessionScoped<WebSharperForestFacadeProxy>(httpContextAccessor)
    member private this.Facade with get() : IForestFacade = upcast this.Current
    member private this.NodeStateProvider with get() : INodeStateProvider = this.Current.Renderer :?> INodeStateProvider
    interface IDocumentStateProvider
        // TODO
    interface INodeStateProvider with 
        member this.ResetStates() = this.NodeStateProvider.ResetStates()
        member this.AllNodes with get() = this.NodeStateProvider.AllNodes
        member this.UpdatedNodes with get() = this.NodeStateProvider.UpdatedNodes
    interface IForestFacade with 
        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = 
            try 
                this.Facade.RegisterSystemView<'sv>()
            with 
            | _ -> this.NodeStateProvider.ResetStates()
        member this.LoadTree tree = 
            try 
                this.Facade.LoadTree tree
            with 
            | _ -> this.NodeStateProvider.ResetStates()
        member this.LoadTree (tree, msg) = 
            try 
                this.Facade.LoadTree (tree, msg)
            with 
            | _ -> this.NodeStateProvider.ResetStates()
        member this.Render renderer result = 
            try 
                this.Facade.Render renderer result
            with 
            | _ -> this.NodeStateProvider.ResetStates()
    interface ICommandDispatcher with 
        member this.ExecuteCommand c h a = 
            try 
                this.Facade.ExecuteCommand c h a
            with 
            | _ -> this.NodeStateProvider.ResetStates()
    interface IMessageDispatcher with 
        member this.SendMessage m = 
            try 
                this.Facade.SendMessage m
            with 
            | _ -> this.NodeStateProvider.ResetStates()
            

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

type [<Sealed;System.Obsolete("Sitelets must use their own facade")>] internal WebSharperForestFacade(forestContext : IForestContext, renderer : IPhysicalViewRenderer<RemotingPhysicalView>) =
    inherit WebSharperForestFacadeProxy(forestContext, renderer)

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
        member __.ResetStates() = allNodes.Clear()
        member __.AllNodes with get() = allNodes.Values |> Array.ofSeq
        member __.UpdatedNodes with get() = allNodes.Values |> Array.ofSeq

    interface IDocumentStateProvider
