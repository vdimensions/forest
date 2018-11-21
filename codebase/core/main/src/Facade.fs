namespace Forest

open Forest.Templates
open Forest.UI

/// An interface allowing communication between the physical application front-end and the Forest UI layer
type [<Interface>] IForestFacade = 
    inherit ICommandDispatcher
    inherit IMessageDispatcher
    abstract member LoadTemplate: template : string -> unit

type [<NoComparison;NoEquality>] DefaultForestFacade<'PV when 'PV :> IPhysicalView>(ctx : IForestContext, renderer : IPhysicalViewRenderer<'PV>) =
    
    let engine = ForestEngine(ctx)

    [<DefaultValue>]
    val mutable private _pvd : PhysicalViewDomProcessor<'PV> voption

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

    abstract member ExecuteCommand: thash -> cname -> obj -> ForestResult
    default __.ExecuteCommand target name arg = engine.Update(fun e -> e.ExecuteCommand target name arg)

    abstract member SendMessage: 'M -> ForestResult
    default __.SendMessage message = engine.Update(fun e -> e.SendMessage message)

    abstract member LoadTemplate: string -> ForestResult
    default __.LoadTemplate template = engine.LoadTemplate template

    interface ICommandDispatcher with
        member this.ExecuteCommand target name arg = this.ExecuteCommand target name arg |> this.Render

    interface IMessageDispatcher with
        member this.SendMessage(message : 'M): unit = this.SendMessage message |> this.Render

    interface IForestFacade with
        member this.LoadTemplate name = name |> this.LoadTemplate |> this.Render
