namespace Forest.Dom
open Forest
open System
open System.Collections.Generic


type [<AutoOpen>] DomNodeType = 
    | Region  = 0b00
    | View    = 0b01
    | Command = 0b10

type IDomNode =
    abstract Name: string with get
    abstract Path: Path.T with get
    abstract Type: DomNodeType with get

[<Interface>]
type ICommandNode =
    inherit IDomNode
    abstract ArgumentType: Type with get

type [<Interface>] IViewNode = 
    inherit IDomNode
    abstract ImplementationType: Type with get
    abstract ViewModelType: Type with get
and [<Interface>] IRegionNode = inherit IDomNode

[<Interface>]
type IDomIndex =
    abstract member Add: node: IDomNode -> IDomIndex
    abstract member Remove: node: IDomNode -> IDomIndex
    abstract member Remove: path: Path.T -> IDomIndex
    abstract member Insert: path: Path.T -> node: IDomNode -> IDomIndex
    abstract member Clear: unit -> IDomIndex
    abstract member Count: int with get
    abstract member Paths: IEnumerable<Path.T> with get
    abstract member Item: Path.T -> Option<IIndex<IDomNode, string>> with get

[<AbstractClass>]
[<AutoOpen>]
type AbstractDomIndex<'T when 'T:> AbstractDomIndex<'T>>() as self =
    abstract member Add: node: IDomNode -> 'T
    abstract member Insert: path: Path.T -> node: IDomNode -> 'T
    abstract member Remove: path: Path.T -> 'T
    abstract member Remove: node: IDomNode -> 'T
    abstract member Clear: unit -> 'T
    abstract member ContainsPath: path: Path.T -> bool 
    abstract member Count: int with get
    abstract member Paths: IEnumerable<Path.T>
    abstract member Item: Path.T -> Option<IIndex<IDomNode, string>> with get
    interface IDomIndex with
        member this.Add node = upcast self.Add node : IDomIndex
        member this.Insert path node = upcast self.Insert path node : IDomIndex
        member this.Remove (path:Path.T) = upcast self.Remove path : IDomIndex
        member this.Remove (node:IDomNode) = upcast self.Remove node : IDomIndex
        member this.Clear () = upcast self.Clear () : IDomIndex
        member this.Count = self.Count
        member this.Paths = self.Paths
        member this.Item with get path = self.[path]

[<Sealed>]
[<AutoOpen>]
type DefaultDomIndex(index: IWriteableIndex<IAutoIndex<IDomNode, string>, Path.T>) as self =
    inherit AbstractDomIndex<DefaultDomIndex>()
    let comparer = StringComparer.Ordinal
    new() = new DefaultDomIndex(new WriteableIndex<IAutoIndex<IDomNode, string>, Path.T>())
    override this.Add node = self.Insert node.Path.Parent node
    override this.Insert path node =
       let mutable nodeIndex: IAutoIndex<IDomNode, string> = 
           upcast new AutoIndex<IDomNode, string>((fun x -> x.Name), (upcast new WriteableIndex<IDomNode, string>(comparer, comparer): IWriteableIndex<IDomNode, string>))
       match index.[path] with | Some ni -> nodeIndex <- ni | None -> ()
       nodeIndex <- nodeIndex.Add node
       new DefaultDomIndex((index.Remove(path).Insert path nodeIndex))

    override this.Remove (path: Path.T) = 
       let newIndex = index.Remove path
       new DefaultDomIndex(newIndex)
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
    override this.Item 
        with get k =
            match index.[k] with
            | Some x -> Some (upcast x: IIndex<IDomNode, string>)
            | None -> None


[<Sealed>]
type internal ViewNode(path: Path.T, name: string, viewType: Type, viewModelType: Type) as self =
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

[<Sealed>]
type internal RegionNode(path: Path.T, name: string) as self =
    member this.Name with get () = name
    member this.Path with get () = path
    interface IRegionNode
    interface IDomNode with
        member this.Name = self.Name
        member this.Path = self.Path
        member this.Type = DomNodeType.Region

[<Sealed>]
type internal CommandNode(path: Path.T, name: string, argumentType: Type) as self = 
    member this.Name with get () = name
    member this.Path with get () = path
    member this.ArgumentType with get () = argumentType
    interface ICommandNode with member this.ArgumentType = self.ArgumentType
    interface IDomNode with
        member this.Name = self.Name
        member this.Path = self.Path
        member this.Type = DomNodeType.Command
