namespace Forest.Dom
open System
open System.Collections.Generic
open Forest



[<Flags>]
type [<AutoOpen>] DomNodeType = 
    | Region  = 0b00
    | View    = 0b01
    | Command = 0b10

type [<AutoOpen>] IDomNode =
    abstract Name: string with get
    abstract Path: Path with get
    abstract Type: DomNodeType with get

type [<AutoOpen>] ICommandNode =
    inherit IDomNode
    abstract ArgumentType: Type with get

type [<AutoOpen>] IViewNode = 
    inherit IDomNode
    abstract ImplementationType: Type with get
    abstract ViewModelType: Type with get
    //abstract Container      : IRegionNode with get
    // TODO: IIndex<string, obj> property for the raw data
    //abstract Regions        : IIndex<IRegionNode, string> with get
    //abstract Commands       : IIndex<ICommandNode, string> with get
and [<AutoOpen>] IRegionNode = 
    inherit IDomNode
    //abstract Parent         : IViewNode with get
    //abstract Views          : IIndex<IViewNode, string> with get

type [<AutoOpen>] IDomIndex =
    abstract member Add: node: IDomNode -> IDomIndex
    abstract member Remove: node: IDomNode -> IDomIndex
    abstract member Remove: path: Path -> IDomIndex
    abstract member Insert: path: Path -> node: IDomNode -> IDomIndex
    abstract member Clear: unit -> IDomIndex
    abstract member Count: int with get
    abstract member Paths: IEnumerable<Path> with get
    abstract member Item: IIndex<IDomNode, string> with get

[<AbstractClass>]
type [<AutoOpen>] AbstractDomIndex<'T when 'T:> AbstractDomIndex<'T>>() =
    abstract member Add: node: IDomNode -> 'T
    abstract member Insert: path: Path -> node: IDomNode -> 'T
    abstract member Remove: path: Path -> 'T
    abstract member Remove: node: IDomNode -> 'T
    abstract member Clear: unit -> 'T
    abstract member ContainsPath: path: Path -> bool 
    abstract member Count: int with get
    abstract member Paths: IEnumerable<Path>
    abstract member Item: IIndex<IDomNode, string> with get
    interface IDomIndex with
        member this.Add node = upcast this.Add node : IDomIndex
        member this.Insert path node = upcast this.Insert path node : IDomIndex
        member this.Remove (path:Path) = upcast this.Remove path : IDomIndex
        member this.Remove (node:IDomNode) = upcast this.Remove node : IDomIndex
        member this.Clear () = upcast this.Clear() : IDomIndex
        member this.Count = this.Count
        member this.Paths = this.Paths
        member this.Item = this.Item

type [<AutoOpen>] DefaultDomIndex(index: IWriteableIndex<IAutoIndex<IDomNode, string>, Path>) =
    inherit AbstractDomIndex<DefaultDomIndex>()
    let comparer = StringComparer.Ordinal
    override this.Add node = this.Insert node.Path.Parent node
    override this.Insert path node =
       let mutable nodeIndex = index.[path]
       if (nodeIndex == null) then 
           nodeIndex <- new AutoIndex<IDomNode, string>((fun x -> x.Name), (upcast new WriteableIndex<IDomNode, string>(comparer, comparer): IWriteableIndex<IDomNode, string>))
           nodeIndex <- nodeIndex.Add node
           ()
       let newIndex = index.Remove(path).Insert path  nodeIndex
       new DefaultDomIndex(newIndex)

    override this.Remove (path: Path) = 
       let newIndex = index.Remove path
       new DefaultDomIndex(newIndex)
    override this.Remove (node: IDomNode) = 
        let path = node.Path
        let parentPath = path.Parent
        let nodeIndex = index.[parentPath].Remove(node.Name)
        // TODO: implement update operation on Index that produces a new index!!!
        this.Remove node.Path
    override this.Clear () = new DefaultDomIndex(index.Clear())
    override this.ContainsPath path = index.ContainsKey path
    override this.Count = index.Count
    override this.Paths = index.Keys
    override this.Item with get () =



type internal ViewNode(dom: IDomIndex, path: Path, name: string, viewType: Type, viewModelType: Type) =
    member this.Name with get () = name
    member this.Path with get () = path
    member this.ImplementationType with get () = viewType
    member this.ViewModelType with get () = viewModelType
    //member this.Regions with get () = dom[path]
    interface IViewNode with
        member this.ImplementationType = this.ImplementationType
        member this.ViewModelType = this.ViewModelType
    interface IDomNode with
        member this.Name = this.Name
        member this.Path = this.Path
        member this.Type = DomNodeType.View

type internal RegionNode(dom: IDomIndex, path: Path, name: string) =
    member this.Name with get () = name
    member this.Path with get () = path
    interface IRegionNode
    interface IDomNode with
        member this.Name = this.Name
        member this.Path = this.Path
        member this.Type = DomNodeType.Region

type internal CommandNode(dom: IDomIndex, path: Path, name: string, argumentType: Type) = 
    member this.Name with get () = name
    member this.Path with get () = path
    member this.ArgumentType with get () = argumentType
    interface ICommandNode with member this.ArgumentType = this.ArgumentType
    interface IDomNode with
        member this.Name = this.Name
        member this.Path = this.Path
        member this.Type = DomNodeType.Command