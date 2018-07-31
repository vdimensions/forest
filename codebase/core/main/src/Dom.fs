namespace Forest.Dom

open Forest

open System
open System.Collections.Generic


type [<AutoOpen>] DomNodeType = 
    | Region  = 0b00
    | View    = 0b01

type [<Interface>] IDomNode =
    abstract Name: string with get
    abstract Path: Path with get
    abstract Type: DomNodeType with get

/// <summary>
/// An interface representing the <see cref="IDomIndex">dom index</see> entry for a forest view.
/// </summary>
/// <seealso cref="IDomIndex"/>
/// <seealso cref="IDomNode"/>
/// <seealso cref="IView"/>
type [<Interface>] IViewNode = 
    inherit IDomNode
    abstract Descriptor: IViewDescriptor with get
/// <summary>
/// An interface representing the <see cref="IDomIndex">dom index</see> entry for a forest region.
/// </summary>
/// <seealso cref="IDomIndex"/>
/// <seealso cref="IDomNode"/>
/// <seealso cref="IRegion"/>
and [<Interface>] IRegionNode = inherit IDomNode

/// <summary>
/// An interface representing forest's Dom Index. The dom index is a container for
/// the elements composing the UI of a forest application, each organized by its <see cref="Path" >path</see>
/// within the view/region structure.
/// </summary>
/// <seealso cref="Path"/>
type [<Interface>] IDomIndex =
    abstract member Add: node: IDomNode -> IDomIndex
    abstract member Remove: node: IDomNode -> IDomIndex
    abstract member Remove: path: Path -> IDomIndex
    abstract member Insert: path: Path -> node: IDomNode -> IDomIndex
    abstract member Clear: unit -> IDomIndex
    abstract member Count: int with get
    abstract member Paths: IEnumerable<Path> with get
    abstract member Item: Path -> Option<IIndex<IDomNode, string>> with get

type [<AbstractClass>] AbstractDomIndex<'T when 'T:> AbstractDomIndex<'T>>() as self =
    abstract member Add: node: IDomNode -> 'T
    abstract member Insert: path: Path -> node: IDomNode -> 'T
    abstract member Remove: path: Path -> 'T
    abstract member Remove: node: IDomNode -> 'T
    abstract member Clear: unit -> 'T
    abstract member ContainsPath: path: Path -> bool 
    abstract member Count: int with get
    abstract member Paths: IEnumerable<Path>
    abstract member Item: Path -> Option<IIndex<IDomNode, string>> with get
    interface IDomIndex with
        member this.Add node = upcast self.Add node : IDomIndex
        member this.Insert path node = upcast self.Insert path node : IDomIndex
        member this.Remove (path:Path) = upcast self.Remove path : IDomIndex
        member this.Remove (node:IDomNode) = upcast self.Remove node : IDomIndex
        member this.Clear () = upcast self.Clear () : IDomIndex
        member this.Count = self.Count
        member this.Paths = self.Paths
        member this.Item with get path = self.[path]

type [<Sealed>] DefaultDomIndex(index: IWriteableIndex<IAutoIndex<IDomNode, string>, Path>) as self =
    inherit AbstractDomIndex<DefaultDomIndex>()
    let comparer = StringComparer.Ordinal
    new() = new DefaultDomIndex(new WriteableIndex<IAutoIndex<IDomNode, string>, Path>())
    override this.Add node = self.Insert node.Path.Parent node
    override this.Insert path node =
       let mutable nodeIndex: IAutoIndex<IDomNode, string> = 
           upcast new AutoIndex<IDomNode, string>((fun x -> x.Name), (upcast new WriteableIndex<IDomNode, string>(comparer, comparer): IWriteableIndex<IDomNode, string>))
       match index.[path] with | Some ni -> nodeIndex <- ni | None -> ()
       nodeIndex <- nodeIndex.Add node
       new DefaultDomIndex((index.Remove(path).Insert path nodeIndex))

    override this.Remove (path: Path) = new DefaultDomIndex((index.Remove path))
    override this.Remove (node: IDomNode) = 
        let path = node.Path
        let parentPath = path.Parent
        let nodeIndex = index.[parentPath];
        match nodeIndex with
        | Some ni when ni.Count > 0 -> new DefaultDomIndex((index.Remove parentPath).Insert parentPath (ni.Remove node))
        | Some ni when ni.Count = 0 -> self.Remove(parentPath)
        | _ -> this
    override this.Clear () = new DefaultDomIndex(index.Clear())
    override this.ContainsPath path = index.ContainsKey path
    override this.Count = index.Count
    override this.Paths = index.Keys
    override this.Item with get k = index.[k] |> Option.map (fun x -> upcast x)


type [<Sealed>] internal ViewNode(path: Path, descriptor: IViewDescriptor) as self =
    member this.Descriptor with get () = descriptor
    member this.Path with get () = path
    //member this.Regions with get () = dom[path]
    interface IViewNode with
        member this.Descriptor = self.Descriptor
    interface IDomNode with
        member this.Name = self.Descriptor.Name
        member this.Path = self.Path
        member this.Type = DomNodeType.View

type [<Sealed>] internal RegionNode(path: Path, name: string) as self =
    member this.Name with get () = name
    member this.Path with get () = path
    interface IRegionNode
    interface IDomNode with
        member this.Name = self.Name
        member this.Path = self.Path
        member this.Type = DomNodeType.Region
