namespace Forest.UI

open System
open Forest

type [<Interface>] IPhysicalView =
    inherit IDisposable
    abstract member Update : node : DomNode -> unit
    abstract member InvokeCommand : name : cname -> arg : obj -> unit
    abstract member Hash : thash

[<Interface>] 
type IPhysicalViewRenderer =
    abstract member CreatePhysicalView: commandDispatcher : ICommandDispatcher -> n : DomNode -> IPhysicalView
    abstract member CreateNestedPhysicalView: commandDispatcher : ICommandDispatcher -> parent : IPhysicalView -> n : DomNode  -> IPhysicalView

[<Interface>]
type IPhysicalViewRenderer<'PV when 'PV :> IPhysicalView> =
    inherit IPhysicalViewRenderer
    abstract member CreatePhysicalViewG: commandDispatcher : ICommandDispatcher -> n : DomNode -> 'PV
    abstract member CreateNestedPhysicalViewG: commandDispatcher : ICommandDispatcher -> parent : 'PV -> n : DomNode  -> 'PV