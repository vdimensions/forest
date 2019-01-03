namespace Forest.Web.WebSharper

open Forest

type [<Interface>] INode =
    abstract member Hash: thash with get
    abstract member Name: vname with get
    abstract member Model: obj with get
    abstract member Regions: Map<rname, INode list> with get

type Node =
    {
        Hash: thash;
        Name: vname;
        Model: obj;
        Regions: Map<rname, Node list>
    }
    with 
        interface INode with
            member this.Hash with get() = this.Hash
            member this.Name with get() = this.Name
            member this.Model with get() = this.Model
            member this.Regions with get() = this.Regions |> Map.map (fun k v -> v |> List.map (fun x -> upcast x))