namespace Forest.UI

open Forest
open Forest.Rendering
open System


type [<Interface>] ICommandDispatcher =
    abstract member InvokeCommand: key:string -> command:cname -> arg:obj -> unit
type [<Interface>] IForestViewAdapter =
    inherit IDisposable
    abstract member Update: viewModel:obj -> unit
    //abstract member AddOrUpdateRegionItem: region:string -> viewModel:obj -> IForestViewAdapter
    abstract member InvokeCommand: name:cname -> arg:obj -> unit
    abstract member Key:string

type [<AbstractClass>] UIAdaptingVisitor() =
    let mutable map: Map<string, IForestViewAdapter> = Map.empty
    let mutable keys: Set<string> = Set.empty

    abstract member CreateAdapter: key:HierarchyKey -> viewModel:obj -> descriptor:IViewDescriptor -> IForestViewAdapter

    member this.Visit (key:HierarchyKey) index viewModel (descriptor:IViewDescriptor) =
        let hash = key.Hash
        match map.TryFind hash with
        | Some adapter -> adapter.Update viewModel
        | None -> 
            let adapter = (this.CreateAdapter key viewModel descriptor)
            adapter.Update viewModel
            map <- map |> Map.add hash adapter
        keys <- keys |> Set.remove hash


    interface IStateVisitor with
        member this.BFS key index viewModel descriptor = 
            this.Visit key index viewModel descriptor
        member this.DFS key index viewModel descriptor = 
            ()
        member this.Done() = 
            for k in keys do 
                match map.TryFind k with
                | Some v -> 
                    v.Dispose()
                    map <- map |> Map.remove k
                | None -> ()
            keys <- map |> Seq.map (fun a -> a.Key) |> Set.ofSeq
            ()

