namespace Forest.UI

open Forest
open Forest.NullHandling

open System


type [<Interface>] IViewRenderer =
    inherit IDisposable
    abstract member Update: viewModel:obj -> unit
    abstract member InvokeCommand: name:cname -> arg:obj -> unit
    abstract member Hash:thash

type [<AbstractClass;NoComparison>] AbstractViewRenderer(commandDispatcher:ICommandDispatcher, key:TreeNode) =
    do 
        ignore <| isNotNull "commandDispatcher" commandDispatcher
        ignore <| isNotNull "key" key

    let mutable vm:obj = nil<obj>
    
    abstract member Refresh: viewModel:obj -> unit
    abstract member Dispose: disposing:bool -> unit

    override this.Finalize() = 
        this.Dispose(false)
        
    interface IViewRenderer with
        member __.InvokeCommand (NotNull "name" name) arg =
            commandDispatcher.ExecuteCommand key.Hash name arg
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
        member this.Dispose() = 
            this.Dispose(true)
            GC.SuppressFinalize(this)
