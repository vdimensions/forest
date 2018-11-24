namespace Forest.Web.WebSharper

open System.Collections.Generic
open Forest
open Forest.UI
open WebSharper.UI

type [<AbstractClass>] WebSharperPhysicalView(dispatcher, hash) =
    inherit AbstractPhysicalView(dispatcher, hash)
    let toDoc (x : WebSharperPhysicalView) = x.Doc()
    let mutable regionMap : Map<rname, WebSharperPhysicalView list> = Map.empty

    member __.GetRegionContent (region : rname) : Doc list =
        match regionMap.TryFind region with
        | None -> []
        | Some list -> list |> List.rev |> List.collect toDoc
    member __.Embed region pv =
        regionMap <- regionMap |> Map.add region (match regionMap.TryFind region with Some data -> pv::data | None -> List.singleton pv)
        pv
    abstract member Doc: unit -> Doc list

type [<AbstractClass>] WebSharperTemplateView<'M, 'T>(dispatcher, hash) =
    inherit WebSharperPhysicalView(dispatcher, hash)
    [<DefaultValue>]
    val mutable private _doc : Doc list voption
    [<DefaultValue>]
    val mutable private _template : 'T voption

    abstract member InstantiateTemplate: unit -> 'T

    override this.Refresh n = 
        this._doc <- ValueNone
        this._template <- this.InstantiateTemplate() |> this.DataBind (n.Model :?> 'M) |> this.HookCommands n.Commands |> ValueSome

    override this.Doc() =
        match (this._doc) with
        | (ValueNone) ->
            let template = match this._template with ValueSome t -> t | ValueNone -> this.InstantiateTemplate()
            let newDoc = this.ToDoc(template)
            this._doc <- ValueSome newDoc
            newDoc
        | (ValueSome d) -> d

    abstract member DataBind: model : 'M -> template : 'T -> 'T
    default __.DataBind _ t = t

    abstract member HookCommands: commands : Map<cname, ICommandModel> -> template : 'T -> 'T
    default __.HookCommands _ t = t

    abstract member ToDoc: 'T -> Doc list

    override __.Dispose _ = ignore()

type [<AbstractClass>] WebSharperDocumentView<'M>(dispatcher, hash) =
    inherit WebSharperTemplateView<'M, Doc list>(dispatcher, hash)
    override __.InstantiateTemplate() = List.empty
    override __.ToDoc template = template
    
type [<Sealed;NoComparison>] private TopLevelPhysicalView (commandDispatcher, hash, origin : WebSharperPhysicalView, list : List<WebSharperPhysicalView>) =
    inherit WebSharperPhysicalView(commandDispatcher, hash)
    override __.Refresh node = 
        origin.Refresh node
    override __.Doc() = 
        origin.Doc()
    override __.Dispose disposing = 
        list.Remove origin |> ignore
        origin.Dispose disposing
    member __.Origin = origin