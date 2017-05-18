namespace Forest.Sdk
open Forest
open Forest.Dom
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

    let (|ViewMatcher|_|) (a: NodeType, b: obj) =
        match a with 
        | View -> 
            match b with
            | Dictionary d -> Some(d)
            | _ -> None
        | _ -> None
    let (|RegionMatcher|_|) (a: NodeType, b: obj) =
        match a with 
        | Region -> 
            match b with
            | Dictionary d -> Some(d)
            | _ -> None
        | _ -> None

    type ViewOrRegion =
        | ViewNode of IViewNode
        | RegionNode of IRegionNode
        | Empty

    // temp:
    let BindViewsToRegion = fun views region  -> region
    let BindRegionsToView = fun regions view  -> view

    let rec internal traverseRawTemplate createView createRegion domIndex region view path name (arg: NodeType*obj) : IDomIndex =
        let recurse = traverseRawTemplate createView createRegion
        match arg with
        | ViewMatcher regionsDictionary -> 
            let regions = new List<IRegionNode>()
            for entry in regionsDictionary do
                let result = recurse region view (path + "/" +  name) entry.Key (Region, entry.Value) 
                match result with
                | RegionNode r -> regions.Add
            let view: IViewNode = createView name region regions
            view = BindRegionsToView regions view 
            ViewNode (view)
        | RegionMatcher viewsDictionary -> 
            let views = new List<IViewNode>()
            for entry in viewsDictionary do
                let result = recurse region view (path + "/" +  name) entry.Key (View, entry.Value)
                match result with
                | ViewNode v -> views.Add          
            let newViews = new AutoIndex<IViewNode, string>(fun a -> a.Name);              
            let region: IRegionNode = createRegion name view
            region = BindViewsToRegion views region 
            RegionNode (region)
        | _ -> ViewOrRegion.Empty
    
    let parseRawTemplate createView createRegion data = 
        traverseRawTemplate createView createRegion new DefaultDomIndex() null null String.Empty String.Empty (Region,data)

        

//[<AbstractClass>]
//type AbstractForestEngine =
//    interface IForestEngine with
//        member x.Execute ctx node =
//            ()