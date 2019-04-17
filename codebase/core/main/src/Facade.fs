namespace Forest

open Forest.Templates
open Forest.UI
open System.Threading

/// An interface allowing communication between the physical application front-end and the Forest UI layer
type [<Interface>] IForestFacade = 
    inherit ICommandDispatcher
    inherit IMessageDispatcher
    abstract member LoadTree: tree : string -> unit
    abstract member LoadTree: tree : string * message : 'msg -> unit
    abstract member RegisterSystemView<'sv when 'sv :> ISystemView> : unit -> unit

type [<NoComparison;NoEquality>] DefaultForestFacade<'PV when 'PV :> IPhysicalView>(ctx : IForestContext, renderer : IPhysicalViewRenderer<'PV>) =
    
    let stateManager = ForestStateManager(ctx)

    [<DefaultValue>]
    val mutable private _pvd : PhysicalViewDomProcessor<'PV> voption
    /// A counter determining the number of nested facade calls.
    /// It will be used to determine whether a render call will occur after a facade call.
    /// For example, if a facade call is called from within the code triggered by another facade call
    /// such as `LoadTree` being called from within `ExecuteCommand` or `SendMessage`, a render call
    /// will not be issued for the `LoadTree` operation, but only for the respective encompassing operation.
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
    default __.ExecuteCommand name target arg = stateManager.Update(fun e -> e.ExecuteCommand name target arg)

    abstract member SendMessage: 'M -> ForestResult
    default __.SendMessage message = stateManager.Update(fun e -> e.SendMessage message)

    abstract member LoadTree: string -> ForestResult
    default __.LoadTree tree = stateManager.LoadTree tree

    abstract member RegisterSystemView<'sv when 'sv :> ISystemView> : unit -> ForestResult
    default __.RegisterSystemView<'sv when 'sv :> ISystemView> () = stateManager.Update (fun e -> e.RegisterSystemView<'sv>() |> ignore)

    interface ICommandDispatcher with
        member this.ExecuteCommand name target arg = 
            Interlocked.Increment(nestingCount) |> ignore
            let result = arg |> this.ExecuteCommand name target
            let nestingLevel = Interlocked.Decrement(nestingCount)
            if nestingLevel = 0 then this.Render result

    interface IMessageDispatcher with
        member this.SendMessage(message : 'M): unit = 
            Interlocked.Increment(nestingCount) |> ignore
            let result = message |> this.SendMessage 
            let nestingLevel = Interlocked.Decrement(nestingCount)
            if nestingLevel = 0 then this.Render result

    interface IForestFacade with
        member this.LoadTree tree = 
            Interlocked.Increment(nestingCount) |> ignore
            let result = tree |> this.LoadTree
            let nestingLevel = Interlocked.Decrement(nestingCount)
            if nestingLevel = 0 then this.Render result

        member this.LoadTree(tree, msg) = 
            Interlocked.Increment(nestingCount) |> ignore
            let mutable result = tree |> this.LoadTree
            result <- this.SendMessage msg
            let nestingLevel = Interlocked.Decrement(nestingCount)
            if nestingLevel = 0 then this.Render result

        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = 
            Interlocked.Increment(nestingCount) |> ignore
            let result = this.RegisterSystemView<'sv>()
            let nestingLevel = Interlocked.Decrement(nestingCount)
            if nestingLevel = 0 then this.Render result
