namespace Forest.Sdk
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
    let (|Dictionary|_|) (v: obj) =  if (v :? IDictionary<string, obj>) then Some (v :?> IDictionary<string, obj>) else None

    let (|ViewMatcher|_|) (a: DomNodeType, b: obj) =
        match a with 
        | DomNodeType.View -> match b with | Dictionary d -> Some(d) | _ -> None
        | _ -> None
    let (|RegionMatcher|_|) (a: DomNodeType, b: obj) =
        match a with 
        | DomNodeType.Region -> match b with | Dictionary d -> Some(d) | _ -> None
        | _ -> None

    let rec traverseRawTemplate (rt: IForestRuntime) (dom: IDomIndex) (path: Path.T) (arg: DomNodeType*obj) : IDomIndex =
        let recurse = traverseRawTemplate 
        let mutable changedDom = dom
        match arg with
        | ViewMatcher regionsDictionary -> 
            for entry in regionsDictionary do
                let name = entry.Key
                let path = path @@ name
                let node = upcast new RegionNode(path, name): IDomNode
                changedDom <- recurse rt (changedDom.Add node) path (node.Type, entry.Value)
            changedDom
        | RegionMatcher viewsDictionary -> 
            for entry in viewsDictionary do
                let name = entry.Key
                let path = path @@ name
                let metadata = rt.Registry.GetViewMetadata name
                let (vt, vmt) =
                    match metadata with
                    | Some m -> (m.ViewType, m.ViewModelType)
                    // TODO: ERROR
                    | None -> (null, null)
                let node = upcast new ViewNode(path, name, vt, vmt): IDomNode
                changedDom <- recurse rt (changedDom.Add node) path (node.Type, entry.Value)
            changedDom
        | _ -> changedDom

    member this.CreateIndex (rt: IForestRuntime, data: obj): IDomIndex = 
        traverseRawTemplate rt (new DefaultDomIndex()) Path.Empty (DomNodeType.Region, data)
