namespace Forest.Sdk
open Forest
open Forest.Dom
open System.Collections.Generic

module RawDataTraverser =
    //let (|EmptyCollection|_|) (collection: ICollection<'a>) = if (collection.Count = 0) then Some collection else None
    let (|Dictionary|_|) (v: obj) =  if (v :? IDictionary<string, obj>) then Some (v :?> IDictionary<string, obj>) else None

    let (|ViewMatcher|_|) (a: DomNodeType, b: obj) =
        match a with 
        | DomNodeType.View -> 
            match b with
            | Dictionary d -> Some(d)
            | _ -> None
        | _ -> None
    let (|RegionMatcher|_|) (a: DomNodeType, b: obj) =
        match a with 
        | DomNodeType.Region -> 
            match b with
            | Dictionary d -> Some(d)
            | _ -> None
        | _ -> None

    type ViewOrRegion = | ViewNode of IViewNode | RegionNode of IRegionNode | Empty

    // temp:
    let rec internal traverseRawTemplate (dom: IDomIndex) (path: Path) (arg: DomNodeType*obj) : IDomIndex =
        let recurse = traverseRawTemplate 
        let mutable changedDom = dom
        match arg with
        | ViewMatcher regionsDictionary -> 
            for entry in regionsDictionary do
                let name = entry.Key
                let path = path.Append(name)
                let node = upcast new RegionNode(path, name): IDomNode
                changedDom <- recurse (changedDom.Add node) path (node.Type, entry.Value)
            changedDom
        | RegionMatcher viewsDictionary -> 
            for entry in viewsDictionary do
                let name = entry.Key
                let path = path.Append(name)
                // TODO: obtain view metadata
                let node = upcast new ViewNode(path, name, null, null): IDomNode
                changedDom <- recurse (changedDom.Add node) path (node.Type, entry.Value)
            changedDom
        | _ -> changedDom
    
    let ParseTemplateStructure data = traverseRawTemplate (new DefaultDomIndex()) Path.Empty (DomNodeType.Region, data)

        

//[<AbstractClass>]
//type AbstractForestEngine =
//    interface IForestEngine with
//        member x.Execute ctx node =
//            ()