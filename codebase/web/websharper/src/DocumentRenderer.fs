namespace Forest.Web.WebSharper

open System.Collections.Generic
open Axle.Web.AspNetCore.Session
open Forest
open Forest.UI
open WebSharper.UI
open Microsoft.AspNetCore.Http


type [<Interface>] IDocumentStateProvider = 
    interface 
    // TODO
    end

type [<AbstractClass;NoComparison>] internal WebSharperForestFacadeProxy private (facade : IForestFacade, renderer : IPhysicalViewRenderer<RemotingPhysicalView>) =
    new(forestContext : IForestContext, renderer : IPhysicalViewRenderer<RemotingPhysicalView>) = WebSharperForestFacadeProxy(DefaultForestFacade<RemotingPhysicalView>(forestContext, renderer), renderer)
    abstract member LoadTree: facade: IForestFacade * name : string -> unit
    default __.LoadTree (facade, name) = facade.LoadTree name
    abstract member LoadTree: facade: IForestFacade * name : string * msg : 't -> unit
    default __.LoadTree (facade, name, msg) = facade.LoadTree (name, msg)
    abstract member SendMessage<'msg> :  facade: IForestFacade -> 'msg -> unit
    default __.SendMessage<'msg> facade msg = facade.SendMessage<'msg> msg
    abstract member ExecuteCommand:  facade: IForestFacade -> cname -> thash -> obj -> unit
    default __.ExecuteCommand facade name hash arg = facade.ExecuteCommand name hash arg
    member __.Renderer with get() = renderer
    interface IForestFacade with
        member this.LoadTree name =
            this.LoadTree (facade, name)
        member this.LoadTree (name, msg) =
            this.LoadTree (facade, name, msg)
        member __.RegisterSystemView<'sv when 'sv :> ISystemView>() =
            facade.RegisterSystemView<'sv>()
    interface IMessageDispatcher with
        member this.SendMessage<'msg> (msg:'msg) =
            this.SendMessage<'msg> facade msg
    interface ICommandDispatcher with
        member this.ExecuteCommand name hash arg =
            this.ExecuteCommand facade name hash arg
            
and [<Sealed;NoComparison>] internal PerSessionWebSharperForestFacade(httpContextAccessor : IHttpContextAccessor) =
    inherit SessionScoped<WebSharperForestFacadeProxy>(httpContextAccessor)
    member private this.Facade with get() : IForestFacade = upcast this.Current
    interface IDocumentStateProvider
        // TODO
    interface INodeStateProvider with 
        member this.AllNodes with get() = (this.Current.Renderer :?> INodeStateProvider).AllNodes
        member this.UpdatedNodes with get() = (this.Current.Renderer :?> INodeStateProvider).UpdatedNodes
    interface IForestFacade with 
        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = this.Facade.RegisterSystemView<'sv>()
        member this.LoadTree tree = this.Facade.LoadTree tree
        member this.LoadTree (tree, msg) = this.Facade.LoadTree (tree, msg)
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
        member __.AllNodes 
            with get() = allNodes.Values |> Array.ofSeq
        member __.UpdatedNodes 
            with get() = allNodes.Values |> Array.ofSeq

    interface IDocumentStateProvider
