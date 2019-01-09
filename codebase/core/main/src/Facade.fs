namespace Forest

open Forest.Templates
open Forest.UI
open System.Threading

/// An interface allowing communication between the physical application front-end and the Forest UI layer
type [<Interface>] IForestFacade = 
    inherit ICommandDispatcher
    inherit IMessageDispatcher
    abstract member LoadTree: tree : string -> unit

type [<NoComparison;NoEquality>] DefaultForestFacade<'PV when 'PV :> IPhysicalView>(ctx : IForestContext, renderer : IPhysicalViewRenderer<'PV>) =
    
    let engine = ForestEngine(ctx)

    [<DefaultValue>]
    val mutable private _pvd : PhysicalViewDomProcessor<'PV> voption
    /// A counter determining the number of nested facade calls.
    /// It will be used to determine whether a render call will occur after a facade call.
    /// For example, if a facade call is called from within the code triggered by another facade call
    /// such as `LoadTemplate` being called from withing `ExecuteCommand` or `SendMessage`, a render call
    /// will not be issued for the `LoadTemplate` operation, but only for the respective encompassing operation.
    [<VolatileField>]
    let mutable nestingCount = ref 0

    abstract member Render: res : ForestResult -> unit
    default this.Render res =
        let domProcessor : IDomProcessor =
            match this._pvd with
            | ValueNone -> 
                let viewDomProcessor = PhysicalViewDomProcessor<'PV>(this, renderer)
                this._pvd <- ValueSome viewDomProcessor
                viewDomProcessor
            | ValueSome viewDomProcessor -> viewDomProcessor
            :> IDomProcessor
        res.Render domProcessor

    abstract member ExecuteCommand: cname -> thash -> obj -> ForestResult
    default __.ExecuteCommand name target arg = engine.Update(fun e -> e.ExecuteCommand name target arg)

    abstract member SendMessage: 'M -> ForestResult
    default __.SendMessage message = engine.Update(fun e -> e.SendMessage message)

    abstract member LoadTree: string -> ForestResult
    default __.LoadTree tree = engine.LoadTree tree

    interface ICommandDispatcher with
        member this.ExecuteCommand name target arg = 
            let before = Interlocked.Increment(nestingCount)
            let result = arg |> this.ExecuteCommand name target
            let after = Interlocked.Decrement(nestingCount)
            if (before - after) = 1 then this.Render result

    interface IMessageDispatcher with
        member this.SendMessage(message : 'M): unit = 
            let before = Interlocked.Increment(nestingCount)
            let result = message |> this.SendMessage 
            let after = Interlocked.Decrement(nestingCount)
            if (before - after) = 1 then this.Render result

    interface IForestFacade with
        member this.LoadTree tree = 
            let before = Interlocked.Increment(nestingCount)
            let result = tree |> this.LoadTree
            let after = Interlocked.Decrement(nestingCount)
            if (before - after) = 1 then this.Render result
