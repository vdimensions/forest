﻿namespace Forest.UI

open Forest

type [<Interface>] ICommandDispatcher =
    abstract member ExecuteCommand : command : cname -> hash : thash -> arg : obj -> unit

type [<Interface>] IMessageDispatcher =
    abstract member SendMessage<'msg> : message : 'msg -> unit

type [<Interface>] ITreeNavigator =
    abstract member LoadTree : string -> unit
    abstract member LoadTree<'msg> : string * 'msg -> unit

type [<Interface>] IForestEngine =
    inherit IMessageDispatcher
    inherit ICommandDispatcher
    inherit ITreeNavigator
    [<System.Obsolete>]
    abstract member RegisterSystemView<'sv when 'sv :> ISystemView> : unit -> 'sv

type [<AbstractClass;NoComparison>] ForestEngineDecorator (engine : IForestEngine) =

    abstract member RegisterSystemView<'sv when 'sv :> ISystemView> : IForestEngine -> 'sv
    default __.RegisterSystemView<'sv when 'sv :> ISystemView> engine = engine.RegisterSystemView<'sv>()

    abstract member LoadTree: IForestEngine * string -> unit
    default __.LoadTree (engine, name) = engine.LoadTree name
    abstract member LoadTree: IForestEngine * string * 'msg -> unit
    default __.LoadTree (engine, name, msg) = engine.LoadTree (name, msg)

    //abstract member Render<'pv when 'pv :> IPhysicalView> : IForestEngine -> IPhysicalViewRenderer<'pv> -> ForestResult-> unit
    //default __.Render<'pv when 'pv :> IPhysicalView> facade renderer result = facade.Render<'pv> renderer result

    abstract member SendMessage<'msg> :  IForestEngine -> 'msg -> unit
    default __.SendMessage<'msg> engine msg = engine.SendMessage<'msg> msg

    abstract member ExecuteCommand: IForestEngine -> cname -> thash -> obj -> unit
    default __.ExecuteCommand engine name hash arg = engine.ExecuteCommand name hash arg

    interface IForestEngine with
        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = this.RegisterSystemView<'sv> engine
    interface ITreeNavigator with
        member this.LoadTree name = this.LoadTree (engine, name)
        member this.LoadTree (name, msg) = this.LoadTree (engine, name, msg)
        //member this.Render renderer result = this.Render facade renderer result
    interface IMessageDispatcher with
        member this.SendMessage<'msg> (msg:'msg) = this.SendMessage<'msg> engine msg
    interface ICommandDispatcher with
        member this.ExecuteCommand name hash arg = this.ExecuteCommand engine name hash arg
