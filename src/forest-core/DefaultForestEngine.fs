namespace Forest
open Forest
open Forest.Dom
open System.Collections.Generic


//[<AbstractClass>]
//type AbstractForestEngine =
//    interface IForestEngine with
//        member x.Execute ctx node =
//            ()

[<Sealed>]
type DefaultForestEngine() = 
    let (|Dictionary|_|) (v: obj) = 
        if (v :? IDictionary<string, obj>) 
        then Some (v :?> IDictionary<string, obj>) 
        else None

    let (|ViewMatcher|_|) (a: DomNodeType, b: obj) =
        match a with 
        | DomNodeType.View -> match b with | Dictionary d -> Some(d) | _ -> None
        | _ -> None
    let (|RegionMatcher|_|) (a: DomNodeType, b: obj) =
        match a with 
        | DomNodeType.Region -> match b with | Dictionary d -> Some(d) | _ -> None
        | _ -> None

    let rec traverseRawTemplate (ctx: IForestContext) (dom: IDomIndex) (path: Path) (arg: DomNodeType*obj) : IDomIndex =
        let mutable changedDom = dom
        match arg with
        | ViewMatcher regionsDictionary -> 
            for entry in regionsDictionary do
                let name = entry.Key
                let path = path @@ name
                let node = upcast new RegionNode(path, name): IDomNode
                changedDom <- traverseRawTemplate ctx (changedDom.Add node) path (node.Type, entry.Value)
            changedDom
        | RegionMatcher viewsDictionary -> 
            for entry in viewsDictionary do
                let name = entry.Key
                let path = path @@ name
                let metadata = ctx.Registry.GetViewMetadata name
                let node = 
                    match metadata with
                    | Some m -> upcast new ViewNode(path, m): IDomNode
                    // TODO: ERROR
                    | None -> upcast new ViewNode(path, View.Descriptor(name, null, null, null)): IDomNode
                
                changedDom <- traverseRawTemplate ctx (changedDom.Add node) path (node.Type, entry.Value)
            changedDom
        | _ -> changedDom

    member this.CreateIndex (ctx: IForestContext, data: obj): IDomIndex = 
        traverseRawTemplate ctx (new DefaultDomIndex()) Path.Empty (DomNodeType.Region, data)

    //let traverseDom

    member this.Execute (cxt: IForestContext, domIndex: IDomIndex) : IDomIndex = 
        for path in domIndex.Paths do
            match domIndex.[path] with
            | Some item ->
                match item with 
                | :? IViewNode as view ->
                    // TODO: process commands
                    // TODO: update viewState
                    // TODO: update domIndex
                    ()
                | :? IRegionNode as region -> 
                    //let r = new Region.T()
                    //
                    ()
            | None -> ()

        Unchecked.defaultof<IDomIndex>

    //let rec materializeDomIndex(ctx: IForestContex, domIndex: IDomIndex) = 


