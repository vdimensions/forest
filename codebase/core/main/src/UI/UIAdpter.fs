namespace Forest.UI

open Forest
open Forest.NullHandling
open Forest.UI.Rendering

open System


type [<Interface>] ICommandDispatcher =
    abstract member InvokeCommand: key:string -> command:cname -> arg:obj -> unit

type [<Interface>] IViewAdapter =
    inherit IDisposable
    abstract member Update: viewModel:obj -> unit
    abstract member InvokeCommand: name:cname -> arg:obj -> unit
    abstract member Key:string

type [<AbstractClass>] AbstractViewAdapter(commandDispatcher:ICommandDispatcher, key:HierarchyKey) =
    do 
        ignore <| isNotNull "commandDispatcher" commandDispatcher
        ignore <| isNotNull "key" key
    let mutable vm:obj = nil<obj>
    
    abstract member Refresh: viewModel:obj -> unit
    abstract member Dispose: disposing:bool -> unit
        
    interface IViewAdapter with
        member __.InvokeCommand (NotNull "name" name) (arg) =
            commandDispatcher.InvokeCommand key.Hash name arg
        member this.Update (NotNull "viewModel" viewModel:obj) =
            let update = 
                match null2vopt vm with
                | ValueSome value -> not <| obj.Equals(value, viewModel)
                | ValueNone -> true
            if update then
                vm <- viewModel
                this.Refresh viewModel
        member __.Key with get() = key.Hash
    interface IDisposable with
        member this.Dispose() = this.Dispose(true)


type [<AbstractClass>] AbstractUIVisitor() =
    let mutable map: Map<string, IViewAdapter> = Map.empty
    let mutable keys: Set<string> = Set.empty

    abstract member CreateAdapter: key:sname -> viewModel:obj -> IViewAdapter

    member this.Visit (key:HierarchyKey) _ viewModel =
        let hash = key.Hash
        match map.TryFind hash with
        | Some adapter -> adapter.Update viewModel
        | None -> 
            let adapter = (this.CreateAdapter key.Hash viewModel)
            adapter.Update viewModel
            map <- map |> Map.add hash adapter
        keys <- keys |> Set.remove hash

    interface IForestRenderer with
        member this.ProcessNode n =
            let hash = n.id
            match map.TryFind hash with
            | Some adapter -> adapter.Update n.model
            | None -> 
                let adapter = (this.CreateAdapter hash n.model)
                adapter.Update n.model
                map <- map |> Map.add hash adapter
            keys <- keys |> Set.remove hash
            n

    interface IForestStateVisitor with
        member this.BFS key index viewModel descriptor = 
            this.Visit key index viewModel
        member __.DFS key index viewModel descriptor = 
            ()
        member __.Complete() = 
            for k in keys do 
                match map.TryFind k with
                | Some v -> 
                    v.Dispose()
                    map <- map |> Map.remove k
                | None -> ()
            keys <- map |> Seq.map (fun a -> a.Key) |> Set.ofSeq
            ()

