namespace Forest.Dom
open Forest
open System
open System.Collections.Generic


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
and [<AutoOpen>] IRegionNode = inherit IDomNode

type [<AutoOpen>] IDomIndex =
    abstract member Add: node: IDomNode -> IDomIndex
    abstract member Remove: node: IDomNode -> IDomIndex
    abstract member Remove: path: Path -> IDomIndex
    abstract member Insert: path: Path -> node: IDomNode -> IDomIndex
    abstract member Clear: unit -> IDomIndex
    abstract member Count: int with get
    abstract member Paths: IEnumerable<Path> with get
    abstract member Item: Path -> IIndex<IDomNode, string> with get

[<AbstractClass>]
type [<AutoOpen>] AbstractDomIndex<'T when 'T:> AbstractDomIndex<'T>>() as self =
    abstract member Add: node: IDomNode -> 'T
    abstract member Insert: path: Path -> node: IDomNode -> 'T
    abstract member Remove: path: Path -> 'T
    abstract member Remove: node: IDomNode -> 'T
    abstract member Clear: unit -> 'T
    abstract member ContainsPath: path: Path -> bool 
    abstract member Count: int with get
    abstract member Paths: IEnumerable<Path>
    abstract member Item: Path -> IIndex<IDomNode, string> with get
    interface IDomIndex with
        member this.Add node = upcast self.Add node : IDomIndex
        member this.Insert path node = upcast self.Insert path node : IDomIndex
        member this.Remove (path:Path) = upcast self.Remove path : IDomIndex
        member this.Remove (node:IDomNode) = upcast self.Remove node : IDomIndex
        member this.Clear () = upcast self.Clear () : IDomIndex
        member this.Count = self.Count
        member this.Paths = self.Paths
        member this.Item with get path = self.[path]


type [<AutoOpen>] DefaultDomIndex(index: IWriteableIndex<IAutoIndex<IDomNode, string>, Path>) =
    inherit AbstractDomIndex<DefaultDomIndex>()
    let comparer = StringComparer.Ordinal
    new() = new DefaultDomIndex(new WriteableIndex<IAutoIndex<IDomNode, string>, Path>())
    override this.Add node = this.Insert node.Path.Parent node
    override this.Insert path node =
       let mutable nodeIndex = index.[path]
       if (box nodeIndex = null) then 
           nodeIndex <- new AutoIndex<IDomNode, string>((fun x -> x.Name), (upcast new WriteableIndex<IDomNode, string>(comparer, comparer): IWriteableIndex<IDomNode, string>))
           ()
       nodeIndex <- nodeIndex.Add node
       let newIndex = index.Remove(path).Insert path nodeIndex
       new DefaultDomIndex(newIndex)

    override this.Remove (path: Path) = 
       let newIndex = index.Remove path
       new DefaultDomIndex(newIndex)
    override this.Remove (node: IDomNode) = 
        let path = node.Path
        let parentPath = path.Parent
        let nodeIndex = index.[parentPath];
        if (box nodeIndex <> null) then
            if (nodeIndex.Count > 0) then new DefaultDomIndex((index.Remove parentPath).Insert parentPath nodeIndex)
            else this.Remove(parentPath)
        else this
    override this.Clear () = new DefaultDomIndex(index.Clear())
    override this.ContainsPath path = index.ContainsKey path
    override this.Count = index.Count
    override this.Paths = index.Keys
    override this.Item with get k = upcast index.[k]: IIndex<IDomNode, string>

type internal ViewNode(path: Path, name: string, viewType: Type, viewModelType: Type) as self =
    member this.Name with get () = name
    member this.Path with get () = path
    member this.ImplementationType with get () = viewType
    member this.ViewModelType with get () = viewModelType
    //member this.Regions with get () = dom[path]
    interface IViewNode with
        member this.ImplementationType = self.ImplementationType
        member this.ViewModelType = self.ViewModelType
    interface IDomNode with
        member this.Name = self.Name
        member this.Path = self.Path
        member this.Type = DomNodeType.View

type internal RegionNode(path: Path, name: string) as self =
    member this.Name with get () = name
    member this.Path with get () = path
    interface IRegionNode
    interface IDomNode with
        member this.Name = self.Name
        member this.Path = self.Path
        member this.Type = DomNodeType.Region

type internal CommandNode(path: Path, name: string, argumentType: Type) as self = 
    member this.Name with get () = name
    member this.Path with get () = path
    member this.ArgumentType with get () = argumentType
    interface ICommandNode with member this.ArgumentType = this.ArgumentType
    interface IDomNode with
        member this.Name = self.Name
        member this.Path = self.Path
        member this.Type = DomNodeType.Command