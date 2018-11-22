namespace Forest.UI

open Forest
open Forest.NullHandling

open System


type [<Interface>] IPhysicalView =
    inherit IDisposable
    abstract member Update : node : DomNode -> unit
    abstract member InvokeCommand : name : cname -> arg : obj -> unit
    abstract member Hash : thash

type [<AbstractClass;NoComparison>] AbstractPhysicalView(commandDispatcher : ICommandDispatcher, hash : thash) =
    do 
        ignore <| isNotNull "commandDispatcher" commandDispatcher
        ignore <| isNotNull "hash" hash

    [<DefaultValue>]
    val mutable private _node : DomNode voption
    
    abstract member Refresh : node : DomNode -> unit
    abstract member Dispose : disposing : bool -> unit

    member __.ExecuteCommand (NotNull "name" name) arg = commandDispatcher.ExecuteCommand hash name arg

    override this.Finalize() = this.Dispose(false)
        
    interface IPhysicalView with
        member this.InvokeCommand (NotNull "name" name) arg = this.ExecuteCommand name arg
        member this.Update (NotNull "node" node : DomNode) =
            let update = 
                match this._node with
                | ValueSome value -> not <| obj.Equals(value, node)
                | ValueNone -> true
            if update then
                this._node <- ValueSome node
                this.Refresh node
        member __.Hash with get() = hash

    interface IDisposable with
        member this.Dispose() = 
            this.Dispose(true)
            GC.SuppressFinalize(this)
