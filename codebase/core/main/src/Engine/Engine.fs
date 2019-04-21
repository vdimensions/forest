namespace Forest

open System
open Forest.UI


module ForestEngine =
    type private T (ctx : IForestContext, sp : IForestStateProvider, renderer : IPhysicalViewRenderer) =

        let wrap (s : State option) (action : IForestEngine -> 'a) : 'a =
            match ForestExecutionContext.Current with
            | ValueSome ec ->
                action ec
            | ValueNone ->
                use ec = ForestExecutionContext.Create(ctx, sp, renderer)
                action ec

        member __.ExecuteCommand command target arg =
            wrap None (fun e -> e.ExecuteCommand command target arg)

        member __.SendMessage msg =
            wrap None (fun e -> e.SendMessage msg)

        member __.LoadTree ( name) =
            wrap None (fun e -> e.LoadTree name)

        member __.LoadTree (name, msg) =
            wrap None (fun e -> e.LoadTree (name, msg))

        [<Obsolete>]
        member __.RegisterSystemView<'sv when 'sv :> ISystemView> () =
            wrap None (fun e -> e.RegisterSystemView<'sv> ())

        interface IForestEngine with
            member this.RegisterSystemView<'sv when 'sv :> ISystemView> () = this.RegisterSystemView<'sv>()
        interface ITreeNavigator with
            member this.LoadTree(name) = this.LoadTree(name)
            member this.LoadTree(name, msg) = this.LoadTree(name, msg)
        interface ICommandDispatcher with
            member this.ExecuteCommand c t a = this.ExecuteCommand c t a
        interface IMessageDispatcher with
            member this.SendMessage msg = this.SendMessage msg

    let internal Create ctx sp renderer = upcast T(ctx, sp, renderer) : IForestEngine