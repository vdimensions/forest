namespace Forest.UI
open System
open Axle.Verification
open Forest


[<AbstractClass;NoComparison>] 
type AbstractPhysicalView(engine : IForestEngine, hash : thash) =
    do 
        ignore <| ``|NotNull|`` "engine" engine
        ignore <| ``|NotNull|`` "hash" hash

    [<DefaultValue>]
    val mutable private _node : DomNode voption
    
    abstract member Refresh : node : DomNode -> unit
    abstract member Dispose : disposing : bool -> unit

    member __.ExecuteCommand (NotNull "name" name) arg = engine.ExecuteCommand name hash arg

    member __.NavigateTo (NotNull "name" name) = engine.LoadTree name
    member __.NavigateTo<'msg> (NotNull "name" name, arg) = engine.LoadTree<'msg>(name, arg)

    override this.Finalize() = this.Dispose(false)
        
    interface IPhysicalView with
        member this.InvokeCommand (NotNull "name" name) arg = this.ExecuteCommand name arg
        member this.NavigateTo(NotNull "name" name) = this.NavigateTo(name)
        member this.NavigateTo<'msg>(NotNull "name" name, arg) = this.NavigateTo<'msg>(name, arg)
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
