namespace Forest.Sdk
open Forest
open System
open System.Collections.Generic

type ForestNode =
    | IViewNode
    | IRegionNode

module internal RawDataTraverser =

    let (|EmptyCollection|_|) (collection: ICollection<'a>) =
        if (collection.Count = 0) then Some collection
        else None
    let (|Dictionary|_|) (v: obj) = 
        if (v :? IDictionary<string, obj>) then Some (v :?> IDictionary<string, obj>) 
        else None

    type NodeType = 
        | View
        | Region

    let (|ViewDict|_|) (a: NodeType, b: obj) =
        match a with 
        | View -> 
            match b with
            | Dictionary d -> Some(d)
            | _ -> None
        |_ -> None
    let (|RegionDict|_|) (a: NodeType, b: obj) =
        match a with 
        | Region -> 
            match b with
            | Dictionary d -> Some(d)
            | _ -> None
        |_ -> None

    type RawTemplateResult =
        | ViewNode of IViewNode
        | RegionNode of IRegionNode
        | Empty

    // temp:
    let BindViewsToRegion = fun region views -> region
    let BindRegionsToView = fun view regions -> view

    let rec cataRawTemplate fView fRegion region view name (arg: NodeType*obj) : RawTemplateResult =
        let recurse = cataRawTemplate fView fRegion
        match arg with
        | ViewDict regionsDictionary -> 
            let regions = new List<IRegionNode>()
            for entry in regionsDictionary do
                let result = recurse region view entry.Key (Region, entry.Value) 
                match result with
                | RegionNode r -> regions.Add
            let view: IViewNode = fView name region regions
            view = BindRegionsToView view regions
            ViewNode (view)
        | RegionDict viewsDictionary -> 
            let views = new List<IViewNode>()
            for entry in viewsDictionary do
                let result = recurse region view entry.Key (View, entry.Value)
                match result with
                | ViewNode v -> views.Add          
            let newViews = new AutoIndex<IViewNode, string>(fun a -> a.Name);              
            let region: IRegionNode = fRegion name view
            region = BindViewsToRegion region views
            RegionNode (region)
        | _ -> Empty
    
    let parseRawTemplate createView createRegion data = 
        cataRawTemplate createView createRegion null null String.Empty (Region,data)

        

//[<AbstractClass>]
//type AbstractForestEngine =
//    interface IForestEngine with
//        member x.Execute ctx node =
//            ()