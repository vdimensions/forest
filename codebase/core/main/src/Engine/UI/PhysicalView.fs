namespace Forest.UI
open System
open Axle.Verification
open Forest
open Forest.Engine


[<AbstractClass;NoComparison>] 
type AbstractPhysicalView(engine : IForestEngine, instanceID : thash) =
    do 
        ignore <| ``|NotNull|`` "engine" engine
        ignore <| ``|NotNull|`` "instanceID" instanceID

    [<DefaultValue>]
    val mutable private _node : DomNode voption
    
    abstract member Refresh : node : DomNode -> unit
    abstract member Dispose : disposing : bool -> unit

    member __.ExecuteCommand ((NotNull "name" name), arg) = engine.ExecuteCommand (name, instanceID, arg)

    member __.NavigateTo (NotNull "name" name) = engine.Navigate name
    member __.NavigateTo<'msg> (NotNull "name" name, arg) = engine.Navigate<'msg>(name, arg)

    override this.Finalize() = this.Dispose(false)
        
    interface IPhysicalView with
        member this.InvokeCommand (NotNull "name" name) arg = this.ExecuteCommand (name, arg)
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
        member __.InstanceID with get() = instanceID

    interface IDisposable with
        member this.Dispose() = 
            this.Dispose(true)
            GC.SuppressFinalize(this)
