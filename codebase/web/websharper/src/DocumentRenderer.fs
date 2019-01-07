namespace Forest.Web.WebSharper

open System.Collections.Generic
open Forest
open Forest.UI
open WebSharper
open WebSharper.UI
open Axle.Web.AspNetCore.Session
open Microsoft.AspNetCore.Http


type [<Interface>] IDocumentRenderer =
    abstract member Doc: unit -> Doc

type [<Interface>] INodeStateProvider =
    abstract member Nodes: Node array with get

type [<Sealed>] internal WebSharperForestFacade(forestContext : IForestContext, renderer : IPhysicalViewRenderer<WebSharperPhysicalViewWrapper>) =
    inherit DefaultForestFacade<WebSharperPhysicalViewWrapper>(forestContext, renderer)
    member __.Renderer with get() = renderer

and [<Sealed>] internal PerSessionWebSharperForestFacade(httpContextAccessor : IHttpContextAccessor) =
    inherit SessionScoped<WebSharperForestFacade>(httpContextAccessor)
    interface INodeStateProvider with
        member this.Nodes with get() = (this.Current.Renderer :?> INodeStateProvider).Nodes

and [<NoComparison>] WebSharperPhysicalViewWrapper internal (commandDispatcher, hash, allNodes : IDictionary<thash, Node>, registry : IWebSharperTemplateRegistry) =
    inherit AbstractPhysicalView(commandDispatcher, hash)
    let mutable regionMap : Map<rname, WebSharperPhysicalViewWrapper list> = Map.empty

    static member domNode2Node dn =
        { Hash = dn.Hash; Name = dn.Name; Model = dn.Model; Regions = dn.Regions |> Map.map (fun _ v -> v |> List.map (fun x -> x.Hash) |> Array.ofList ) |> Map.toArray}

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

type [<Sealed;NoEquality;NoComparison>] Remoting =
    [<DefaultValue>]
    static val mutable private _facade : PerSessionWebSharperForestFacade voption
    [<DefaultValue>]
    static val mutable private _nodeProvider : INodeStateProvider voption

    static member internal Init (forest : PerSessionWebSharperForestFacade) =
        match Remoting._facade with
        | ValueNone -> 
            Remoting._facade <- ValueSome forest
            Remoting._nodeProvider <- ValueSome <| upcast forest
        | ValueSome _ -> invalidOp "A forest facade is already initialized"

    static member Facade 
        with get() = 
            match Remoting._facade with
            | ValueSome f -> f.Current :> IForestFacade
            | ValueNone -> invalidOp "A forest facade has not been initialized yet"

    [<Rpc>]
    static member GetNodes () : Async<Node array> = 
        async {
            let nodes =
                match Remoting._nodeProvider with
                | ValueSome p -> p.Nodes
                | ValueNone -> Array.empty
            return nodes
        }
    [<Rpc>]
    static member ExecuteCommand hash cmd (arg : obj) : unit = 
        async {
            Remoting.Facade.ExecuteCommand hash cmd arg |> ignore
        }
        |> Async.Start

[<JavaScript>]
module ClientCode =
    open WebSharper.UI.Client
    open WebSharper.UI.Html

    let private _nodes : Var<Node array> = Var.Create <| Array.empty
    let private _views : Var<Map<vname, WebSharperPhysicalView>> = Var.Create <| Map.empty

    let setNodes nodes = nodes |> _nodes.Set
    let setViews nodes = nodes |> _views.Set

    let private syncNodes () : unit =
        async {
            let! n = Remoting.GetNodes()
            n |> _nodes.Set
        }
        |> Async.Start

    let afterRender (_ : JavaScript.Dom.Element) = 
        if (_nodes.Value.Length = 0) then syncNodes()

    let forestInit() : Doc =
        div [ on.afterRender <@ afterRender @> ] []
    
    // --------------------------------------
    let private tree () : View<Map<thash,Node*WebSharperPhysicalView>> =
        View.Map2
            (fun (a : Node seq) b -> 
                a 
                |> Seq.map (fun (a) -> a.Hash, a, (b |> Map.tryFind a.Name))
                |> Seq.map (fun (a, b, c) -> match c with | None -> None | Some c1 -> Some (a, (b, c1)))
                |> Seq.choose id
                |> Map.ofSeq
            )
            (_nodes.View |> View.MapSeqCached id)
            (_views.View)
        
    let private directDoc (docView : View<Doc>) = 
        docView |> Doc.EmbedView
    let inline private clientDoc (docView : View<Doc>) : Doc =
        client <@ docView |> directDoc @>

    let rec private processDocInternal (t : Map<thash,Node*WebSharperPhysicalView>) (hash : thash) : Doc =
        match t |> Map.tryFind hash with
        | None -> Doc.Empty
        | Some (node, pv) ->
            let regionDocs = 
                node.Regions 
                |> Array.map (fun (rname, v) -> rname, v |> Array.map (fun x -> processDocInternal t x) |> Doc.Concat)
            pv.Doc regionDocs node
    let private processDoc (wrapper : View<Doc> -> Doc) (tree : View<Map<thash,Node*WebSharperPhysicalView>>) (hash : thash) : Doc =
        tree |> View.Map (fun t -> processDocInternal t hash) |> wrapper
    let render hash =
        client <@ tree() |> View.Map (fun t -> processDocInternal t hash) |> Doc.EmbedView @>

    let renderRegionsOnServer (rdata : array<rname * thash array>) = 
        rdata |> Array.map (fun (r, hs) -> r, hs |> Array.map (fun h -> processDoc clientDoc (tree()) h) |> Doc.Concat )
    let renderRegionsOnClient (rdata : array<rname * thash array>) = 
        rdata |> Array.map (fun (r, hs) -> r, hs |> Array.map (fun h -> processDoc directDoc (tree()) h) |> Doc.Concat)
    // --------------------------------------

    let executeCommand hash cmd (arg : obj) =
        Remoting.ExecuteCommand hash cmd arg
        syncNodes()

type [<Sealed;NoComparison>] internal WebSharperTopLevelPhysicalViewWrapper(commandDispatcher, hash, topLevelViews: List<WebSharperTopLevelPhysicalViewWrapper>, allNodes, registry) =
    inherit WebSharperPhysicalViewWrapper(commandDispatcher, hash, allNodes, registry)
    member private __.base_Dispose disposing = base.Dispose disposing
    override this.Dispose disposing = 
        topLevelViews.Remove this |> ignore
        this.base_Dispose disposing
    member __.Node with get() = allNodes.[hash]

type [<Sealed;NoComparison>] WebSharperPhysicalViewRenderer(registry : IWebSharperTemplateRegistry) =
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

    interface IDocumentRenderer with 
        member __.Doc() = 
            match topLevelViews |> List.ofSeq |> List.map (fun x -> x.Doc (x.Node.Regions |> ClientCode.renderRegionsOnServer)) with
            | [] -> Doc.Empty
            | [x] -> x
            | list -> list |> Doc.Concat
            //|> List.tryHead
            //|> Option.defaultWith (fun () -> Doc.Empty)
    interface INodeStateProvider with
        member __.Nodes with get() = allNodes.Values |> Array.ofSeq
