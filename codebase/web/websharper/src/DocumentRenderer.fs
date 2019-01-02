namespace Forest.Web.WebSharper

open System.Collections.Generic
open Forest
open Forest.UI
open WebSharper.UI
open WebSharper.UI.Client
    

type [<Interface>] IDocumentRenderer =
    abstract member Doc: unit -> Doc
    [<System.Obsolete>]
    abstract member DocVar: unit -> Var<Doc>

type [<NoComparison>] WebSharperPhysicalViewWrapper internal (commandDispatcher, hash, registry : IWebSharperTemplateRegistry, state : Var<Map<thash, Doc>>) =
    inherit AbstractPhysicalView(commandDispatcher, hash)
    let mutable regionMap : Map<rname, WebSharperPhysicalViewWrapper list> = Map.empty

    override __.Refresh node = 
        let map = state.Value
        state.Value <- 
            match map.TryFind hash with
            | Some _ -> map |> Map.remove node.Hash
            | None -> map 
            |> Map.add node.Hash (node |> WebSharperPhysicalViewWrapper.toDoc registry regionMap)

    member __.Embed region pv =
        regionMap <- regionMap |> Map.add region (match regionMap.TryFind region with Some data -> pv::data | None -> List.singleton pv)
        pv

    override __.Dispose _ = 
        state.Value <- state.Value |> Map.remove hash

    static member private toDoc (registry : IWebSharperTemplateRegistry) (regionMap : Map<rname, WebSharperPhysicalViewWrapper list>) (node : DomNode) : Doc =
        let inline mapPVWrapper (pvw : WebSharperPhysicalViewWrapper list) = 
            pvw |> List.map (fun x -> x :> IDocProvider )
        let inline mapRegionContents (rm : Map<rname, WebSharperPhysicalViewWrapper list>) = 
            rm |> Map.map (fun _ v -> v |> mapPVWrapper)
        let rm = regionMap
        node |> (registry.Get >> (fun pv -> rm |> mapRegionContents |> pv.Doc))

    member __.Doc () : Doc =
        let h = hash
        state.Value.TryFind h |> Option.defaultWith (fun () -> Doc.Empty)

    member __.DocView () : View<Doc> =
        let h = hash
        //state.View.MapCached (fun m -> m.TryFind h |> Option.defaultWith (fun () -> Doc.Empty) )
        state.View.MapSeqCached (fun a b -> ())

    member __.DocVar () : Var<Doc> =
        let h = hash
        state.Lens (fun m -> m.TryFind h |> Option.defaultWith (fun () -> Doc.Empty)) (fun a _ -> a)

    interface IDocProvider with
        member this.Doc() = this.Doc()
        member this.DocView() = this.DocView()
        member this.DocVar() = this.DocVar()

type [<Sealed;NoComparison>] internal WebSharperTopLevelPhysicalViewWrapper(commandDispatcher, hash, registry : IWebSharperTemplateRegistry, state, topLevelViews : List<WebSharperTopLevelPhysicalViewWrapper>) =
    inherit WebSharperPhysicalViewWrapper(commandDispatcher, hash, registry, state)
    member private __.base_Dispose disposing = base.Dispose disposing
    override this.Dispose disposing = 
        topLevelViews.Remove this |> ignore
        this.base_Dispose disposing

type [<Sealed;NoComparison>] WebSharperPhysicalViewRenderer(registry : IWebSharperTemplateRegistry) =
    inherit AbstractPhysicalViewRenderer<WebSharperPhysicalViewWrapper>()
    let state : Var<Map<thash, Doc>> = Var.Create Map.empty
    let topLevelViews = List<WebSharperTopLevelPhysicalViewWrapper>()

    override __.CreatePhysicalView commandDispatcher domNode = 
        let result = new WebSharperTopLevelPhysicalViewWrapper(commandDispatcher, domNode.Hash, registry, state, topLevelViews)
        topLevelViews.Add result
        upcast result

    override __.CreateNestedPhysicalView commandDispatcher parent domNode =
        let embedFn = (match parent with :? WebSharperTopLevelPhysicalViewWrapper as t -> t.Embed domNode.Region | _ -> id)
        new WebSharperPhysicalViewWrapper(commandDispatcher, domNode.Hash, registry, state)
        |> embedFn

    interface IDocumentRenderer with 
        member __.Doc() = 
            match topLevelViews |> List.ofSeq |> List.map (fun x -> x.Doc()) with
            | [] -> Doc.Empty
            | [x] -> x
            | list -> list |> Doc.Concat
            //|> List.tryHead
            //|> Option.defaultWith (fun () -> Doc.Empty)
        member __.DocVar() = 
            match topLevelViews |> List.ofSeq |> List.map (fun x -> x.DocVar()) with
            | [] -> [Var.Create Doc.Empty]
            | [x] -> [x]
            | list -> list
            |> List.tryHead
            |> Option.defaultWith (fun () -> Var.Create Doc.Empty)
