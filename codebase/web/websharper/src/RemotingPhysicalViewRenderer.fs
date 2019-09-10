namespace Forest.Web.WebSharper

open System.Collections.Generic
open Microsoft.AspNetCore.Http
open WebSharper.UI
open Axle.Web.AspNetCore.Session
open Forest
open Forest.UI
open Forest.StateManagement


type [<Interface>] IDocumentStateProvider = 
    interface 
    // TODO
    end

and [<NoComparison;NoEquality>] internal RemotingPhysicalView (engine, hash, allNodes : IDictionary<thash, Node>) =
    inherit AbstractPhysicalView(engine, hash)
    let mutable regionMap : Map<rname, RemotingPhysicalView list> = Map.empty

    static member domNode2Node (dn : DomNode) =
        { 
            Hash = dn.InstanceID
            Name = dn.Name
            Model = dn.Model
            Regions = dn.Regions |> Seq.map(|KeyValue|) |> Seq.map (fun (k, v) -> k, v |> Seq.map (fun x -> x.InstanceID) |> Array.ofSeq ) |> Seq.toArray;
            Commands = dn.Commands |> Seq.map(|KeyValue|) |> Seq.map (fun (k, c) -> k, { Name = c.Name; DisplayName = c.DisplayName; ToolTip = c.Tooltip; Description = c.Description }) |> Seq.toArray;
            Links = dn.Links |> Seq.map(|KeyValue|) |> Seq.map (fun (k, l) -> k, { Href= ""; Name = l.Name; DisplayName = l.DisplayName; ToolTip = l.Tooltip; Description = l.Description }) |> Seq.toArray;
        }

    override __.Refresh node = 
        allNodes.[node.InstanceID] <- node |> RemotingPhysicalView.domNode2Node

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

    override __.CreatePhysicalView (engine, domNode) = 
        // TODO: pass context
        let result = new RemotingRootPhysicalView(engine, domNode.InstanceID, topLevelViews, allNodes)
        topLevelViews.Add result
        upcast result

    override __.CreateNestedPhysicalView (engine, parent, domNode) =
        // TODO: pass context
        new RemotingPhysicalView(engine, domNode.InstanceID, allNodes)
        |> parent.Embed domNode.Region

    interface INodeStateProvider with
        member __.ResetStates() = allNodes.Clear()
        member __.AllNodes with get() = allNodes.Values |> Array.ofSeq
        member __.UpdatedNodes with get() = allNodes.Values |> Array.ofSeq

    interface IDocumentStateProvider


// TODO: pass context
type WebSharperForestState private (state : ForestState, renderer : WebSharperPhysicalViewRenderer, syncRoot : obj) =
    internal new (state) = WebSharperForestState (state, WebSharperPhysicalViewRenderer(), obj())
    internal new () = WebSharperForestState (ForestState())
    static member ReplaceState (state : ForestState) (fws : WebSharperForestState) = 
        WebSharperForestState(state, fws.Renderer, fws.SyncRoot)
    member __.State with get() = state
    member internal __.SyncRoot = syncRoot
    member internal __.Renderer = renderer

type [<Sealed;NoComparison>] WebSharperSessionStateProvider(httpContextAccessor : IHttpContextAccessor) =
    inherit SessionScoped<WebSharperForestState>(httpContextAccessor)

    interface IPhysicalViewRenderer with
        member this.CreatePhysicalView (commandDispatcher, node) =
            (this.Current.Renderer :> IPhysicalViewRenderer).CreatePhysicalView(commandDispatcher, node)
        member this.CreateNestedPhysicalView(commandDispatcher, parent, node) =
            (this.Current.Renderer :> IPhysicalViewRenderer).CreateNestedPhysicalView(commandDispatcher, parent, node)

    interface INodeStateProvider with
        member this.ResetStates() = (this.Current.Renderer :> INodeStateProvider).ResetStates()
        member this.AllNodes with get() = (this.Current.Renderer :> INodeStateProvider).AllNodes
        member this.UpdatedNodes with get() = (this.Current.Renderer :> INodeStateProvider).UpdatedNodes

    interface IForestStateProvider with
        member this.LoadState () = 
            // TODO: pass context
            this.AddOrReplace(
                httpContextAccessor.HttpContext.Session.Id,
                WebSharperForestState(ForestState()),
                new System.Func<WebSharperForestState, WebSharperForestState, WebSharperForestState>(fun existing _ -> existing))
            let v = this.Current
            System.Threading.Monitor.Enter v.SyncRoot
            v.State
        member this.CommitState state =
            // TODO: pass context
            this.AddOrReplace(
                httpContextAccessor.HttpContext.Session.Id,
                this.Current |> WebSharperForestState.ReplaceState state,
                new System.Func<WebSharperForestState, WebSharperForestState, WebSharperForestState>(fun _ newState -> newState)
            )
            System.Threading.Monitor.Exit this.Current.SyncRoot
        member this.RollbackState () =
            System.Threading.Monitor.Exit this.Current.SyncRoot
