namespace Forest

open Forest
open Forest.Dom

open System
open System.Collections.Generic


module Engine =
    type Operation =
        | AddView of View.Path

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

    let rec private traverseRawTemplate (ctx: IForestContext) (dom: IDomIndex) (path: Path) (arg: DomNodeType*obj) : IDomIndex =
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
                let metadata = ctx.ViewRegistry.GetViewMetadata name
                let node = 
                    match metadata with
                    | Some m -> upcast new ViewNode(path, m): IDomNode
                    // TODO: ERROR
                    | None -> upcast new ViewNode(path, View.Descriptor(name, null, null, null)): IDomNode
                
                changedDom <- traverseRawTemplate ctx (changedDom.Add node) path (node.Type, entry.Value)
            changedDom
        | _ -> changedDom


    let CreateIndex (ctx: IForestContext, data: obj): IDomIndex = 
        traverseRawTemplate ctx (new DefaultDomIndex()) Path.Empty (DomNodeType.Region, data)

    let Execute (cxt: IForestContext, domIndex: IDomIndex) : IDomIndex = 
        for path in domIndex.Paths do
            match domIndex.[path] with
            | Some x ->
                for item in x do
                    match item with
                    | :? IViewNode as view ->
                        let parentPathContentCount = "0"
                        let viewPath = (((path @@ "#") @@ parentPathContentCount) @@ "#") @@ view.Name
                        // TODO: process commands
                        // TODO: update viewState
                        // TODO: update domIndex
                        ()
                    | :? IRegionNode as region -> 
                        let regionPath = path @@ region.Name
                        //let r = new Region.T()
                        //
                        ()
                    | _ -> ()
            | None -> ()
        Unchecked.defaultof<IDomIndex>

    let Instantiate(ctx: IForestContext) : unit =
        // 1. Construct/update a linear hierarchical representation of the current state.
        //    Representation is as follows: path -> forestState
        //       path: region/index#view
        //       forestState: viewInstance * viewDescriptor * viewModelState
        //
        // 2. Detect 

        //let viewInstance = ctx.ViewRegistry.Resolve(viewName)

        //let mutable vsIndex = WritableIndex<ViewState, Guid>()
        //vsIndex.Insert

        ()