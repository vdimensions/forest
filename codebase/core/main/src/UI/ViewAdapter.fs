namespace Forest.UI

open Forest
open Forest.NullHandling

open System


type [<Interface>] ICommandDispatcher =
    abstract member InvokeCommand: hash:hash -> command:cname -> arg:obj -> unit

type [<Interface>] IViewAdapter =
    inherit IDisposable
    abstract member Update: viewModel:obj -> unit
    abstract member InvokeCommand: name:cname -> arg:obj -> unit
    abstract member Hash:hash

type [<AbstractClass>] AbstractViewAdapter(commandDispatcher:ICommandDispatcher, key:TreeNode) =
    do 
        ignore <| isNotNull "commandDispatcher" commandDispatcher
        ignore <| isNotNull "key" key

    let mutable vm:obj = nil<obj>
    
    abstract member Refresh: viewModel:obj -> unit
    abstract member Dispose: disposing:bool -> unit
        
    interface IViewAdapter with
        member __.InvokeCommand (NotNull "name" name) arg =
            commandDispatcher.InvokeCommand key.Hash name arg
        member this.Update (NotNull "model" model:obj) =
            let update = 
                match null2vopt vm with
                | ValueSome value -> not <| obj.Equals(value, model)
                | ValueNone -> true
            if update then
                vm <- model
                this.Refresh model
        member __.Hash with get() = key.Hash
    interface IDisposable with
        member this.Dispose() = this.Dispose(true)
