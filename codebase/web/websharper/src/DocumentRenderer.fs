namespace Forest.Web.WebSharper

open System.Collections.Generic
open Forest
open Forest.UI
open WebSharper.UI
    

type [<Interface>] IDocumentRenderer =
    abstract member Doc: unit -> Doc

type [<Sealed;NoComparison>] WebSharperPhysicalViewRenderer(registry : IWebSharperTemplateRegistry) =
    inherit AbstractPhysicalViewRenderer<WebSharperPhysicalView>()
    let list = List<WebSharperPhysicalView>()

    override __.CreatePhysicalView commandDispatcher domNode = 
        let origin = domNode |> registry.Get commandDispatcher 
        let result = new TopLevelPhysicalView(commandDispatcher, domNode.Hash, origin, list)
        list.Add origin
        upcast result

    override __.CreateNestedPhysicalView commandDispatcher parent domNode =
        domNode
        |> registry.Get commandDispatcher
        |> (match parent with :? TopLevelPhysicalView as t -> t.Origin | _ -> parent).Embed domNode.Region

    interface IDocumentRenderer with 
        member __.Doc() = 
            match list |> Seq.collect (fun x -> x.Doc()) |> Seq.tryHead with
            | Some doc -> doc
            | None -> Doc.Empty
