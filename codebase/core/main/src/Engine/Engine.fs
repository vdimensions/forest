namespace Forest

open System.Diagnostics
open Axle.Logging
open Forest.Engine
open Forest.UI

module ForestEngine =

    type private T (ctx : IForestContext, sp : IForestStateProvider, renderer : IPhysicalViewRenderer, logger : ILogger) =

        let log (op : string) action engine =
            let sw = Stopwatch.StartNew()
            let result = action engine
            sw.Stop()
            logger.Trace("Forest '{0}' operation took {1}ms to complete. ", op, sw.ElapsedMilliseconds)
            result

        let wrap op (engine : IForestEngine) (action : IForestEngine -> 'a) : 'a =
            match ForestExecutionContext.Current with
            | ValueSome ec ->
                log op action ec
            | ValueNone ->
                use ec = ForestExecutionContext.Create(ctx, sp, PhysicalViewDomProcessor(engine, renderer))
                log op action ec

        member this.ExecuteCommand (command, target, arg) =
            wrap "ExecuteCommand" this (fun e -> e.ExecuteCommand(command, target, arg))

        member this.SendMessage msg =
            wrap "SendMessage" this (fun e -> e.SendMessage msg)

        member this.LoadTree ( name) =
            wrap "LoadTree" this (fun e -> e.Navigate name)

        member this.LoadTree (name, msg) =
            wrap "LoadTree" this (fun e -> e.Navigate (name, msg))

        member this.RegisterSystemView<'sv when 'sv :> ISystemView> () =
            wrap "RegisterSystemView" this (fun e -> e.RegisterSystemView<'sv> ())

        interface IForestEngine with
            member this.RegisterSystemView<'sv when 'sv :> ISystemView> () = this.RegisterSystemView<'sv>()
        interface ITreeNavigator with
            member this.Navigate(name) = this.LoadTree(name)
            member this.Navigate(name, msg) = this.LoadTree(name, msg)
        interface ICommandDispatcher with
            member this.ExecuteCommand (c, t, a) = this.ExecuteCommand (c, t, a)
        interface IMessageDispatcher with
            member this.SendMessage msg = this.SendMessage msg

    let internal Create ctx sp renderer logger = upcast T(ctx, sp, renderer, logger) : IForestEngine