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
    do ignore <| isNotNull "commandDispatcher" commandDispatcher
    let mutable vm:obj = nil<obj>
    
    abstract member Refresh: viewModel:obj -> unit
    abstract member Dispose: disposing:bool -> unit
        
    interface IViewAdapter with
        member __.InvokeCommand (NotNull "name" name) (arg) =
            commandDispatcher.InvokeCommand key.Hash name arg
        member this.Update (NotNull "viewModel" viewModel:obj) =
            match null2vopt vm with
            | ValueSome vm -> if not <| obj.Equals(vm, viewModel) then this.Refresh viewModel
            | ValueNone -> this.Refresh viewModel
        member __.Key with get() = key.Hash
    interface IDisposable with
        member this.Dispose() = this.Dispose(true)



type [<AbstractClass>] AbstractUIVisitor() =
    let mutable map: Map<string, IViewAdapter> = Map.empty
    let mutable keys: Set<string> = Set.empty

    abstract member CreateAdapter: key:HierarchyKey -> viewModel:obj -> descriptor:IViewDescriptor -> IViewAdapter

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

