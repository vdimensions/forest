namespace Forest.Web.WebSharper

open System.Collections.Generic
open Microsoft.AspNetCore.Http
open WebSharper.UI
open Axle.Web.AspNetCore.Session
open Forest
open Forest.UI


type [<Interface>] IDocumentStateProvider = 
    interface 
    // TODO
    end

and [<NoComparison;NoEquality>] internal RemotingPhysicalView (engine, hash, allNodes : IDictionary<thash, Node>) =
    inherit AbstractPhysicalView(engine, hash)
    let mutable regionMap : Map<rname, RemotingPhysicalView list> = Map.empty

    static member domNode2Node (dn : DomNode) =
        { 
            Hash = dn.Hash; 
            Name = dn.Name; 
            Model = dn.Model; 
            Regions = dn.Regions |> Map.map (fun _ v -> v |> List.map (fun x -> x.Hash) |> Array.ofList ) |> Map.toArray;
            Commands = dn.Commands |> Map.map<cname, ICommandModel, CommandNode> (fun _ c -> { Name = c.Name; DisplayName = c.DisplayName; ToolTip = c.Tooltip; Description = c.Description }) |> Map.toArray;
            Links = dn.Links |> Map.map<string, ILinkModel, LinkNode> (fun _ l -> { Name = l.Name; DisplayName = l.DisplayName; ToolTip = l.Tooltip; Description = l.Description }) |> Map.toArray;
        }

    override __.Refresh node = 
        allNodes.[node.Hash] <- node |> RemotingPhysicalView.domNode2Node

    member __.Embed region pv =
        regionMap <- regionMap |> Map.add region (match regionMap.TryFind region with Some data -> pv::data | None -> List.singleton pv)
        pv

    override __.Dispose _ = 
        hash |> allNodes.Remove |> ignore

type [<Sealed;NoEquality;NoComparison>] internal RemotingRootPhysicalView(engine, hash, topLevelViews: List<RemotingRootPhysicalView>, allNodes) =
    inherit RemotingPhysicalView(engine, hash, allNodes)
    member private __.base_Dispose disposing = base.Dispose disposing
    override this.Dispose disposing = 
        topLevelViews.Remove this |> ignore
        this.base_Dispose disposing

type [<Sealed;NoEquality;NoComparison>] internal WebSharperPhysicalViewRenderer() =
    inherit AbstractPhysicalViewRenderer<RemotingPhysicalView>()
    let topLevelViews = List<RemotingRootPhysicalView>()
    let allNodes = Dictionary<thash,Node>(System.StringComparer.Ordinal)

    override __.CreatePhysicalView engine domNode = 
        let result = new RemotingRootPhysicalView(engine, domNode.Hash, topLevelViews, allNodes)
        topLevelViews.Add result
        upcast result

    override __.CreateNestedPhysicalView engine parent domNode =
        new RemotingPhysicalView(engine, domNode.Hash, allNodes)
        |> parent.Embed domNode.Region

    interface INodeStateProvider with
        member __.ResetStates() = allNodes.Clear()
        member __.AllNodes with get() = allNodes.Values |> Array.ofSeq
        member __.UpdatedNodes with get() = allNodes.Values |> Array.ofSeq

    interface IDocumentStateProvider


type WebSharperForestState private (state : State, renderer : WebSharperPhysicalViewRenderer, syncRoot : obj) =
    internal new (state) = WebSharperForestState (state, WebSharperPhysicalViewRenderer(), obj())
    internal new () = WebSharperForestState (State.initial)
    static member ReplaceState (state : State) (fws : WebSharperForestState) = 
        WebSharperForestState(state, fws.Renderer, fws.SyncRoot)
    member __.State with get() = state
    member internal __.SyncRoot = syncRoot
    member internal __.Renderer = renderer

type [<Sealed;NoComparison>] WebSharperSessionStateProvider(httpContextAccessor : IHttpContextAccessor) =
    inherit SessionScoped<WebSharperForestState>(httpContextAccessor)

    interface IPhysicalViewRenderer with
        member this.CreatePhysicalView commandDispatcher node =
            (this.Current.Renderer :> IPhysicalViewRenderer).CreatePhysicalView commandDispatcher node
        member this.CreateNestedPhysicalView commandDispatcher parent node =
            (this.Current.Renderer :> IPhysicalViewRenderer).CreateNestedPhysicalView commandDispatcher parent node

    interface INodeStateProvider with
        member this.ResetStates() = (this.Current.Renderer :> INodeStateProvider).ResetStates()
        member this.AllNodes with get() = (this.Current.Renderer :> INodeStateProvider).AllNodes
        member this.UpdatedNodes with get() = (this.Current.Renderer :> INodeStateProvider).UpdatedNodes

    interface IForestStateProvider with
        member this.LoadState () = 
            this.AddOrReplace(
                httpContextAccessor.HttpContext.Session.Id,
                WebSharperForestState(State.initial),
                new System.Func<WebSharperForestState, WebSharperForestState, WebSharperForestState>(fun existing _ -> existing))
            let v = this.Current
            System.Threading.Monitor.Enter v.SyncRoot
            v.State
        member this.CommitState state =
            this.AddOrReplace(
                httpContextAccessor.HttpContext.Session.Id,
                this.Current |> WebSharperForestState.ReplaceState state,
                new System.Func<WebSharperForestState, WebSharperForestState, WebSharperForestState>(fun _ newState -> newState)
            )
            System.Threading.Monitor.Exit this.Current.SyncRoot
        member this.RollbackState () =
            System.Threading.Monitor.Exit this.Current.SyncRoot
