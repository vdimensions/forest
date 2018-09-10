namespace Forest

open Forest.Collections

open System
open System.Collections.Generic


type [<Interface>] IViewDescriptor = 
    abstract Name:vname with get
    abstract ViewType:Type with get
    abstract ViewModelType:Type with get
    abstract Commands:Index<ICommandDescriptor, cname> with get
    abstract Events:IEnumerable<IEventDescriptor> with get

 and [<Interface>] ICommandDescriptor = 
    abstract Name:cname with get
    abstract ArgumentType:Type with get
    abstract member Invoke: arg:obj -> v:IView -> unit

 and [<Interface>] IEventDescriptor =
    abstract Topic:string with get
    abstract MessageType:Type with get
    abstract member Trigger: view:IView -> message:obj -> unit

 and [<Interface>] IViewRegistry =
    abstract member Register: t:Type -> IViewRegistry
    abstract member Register<'T when 'T:>IView> : unit -> IViewRegistry
    abstract member Resolve: name:vname -> IView
    abstract member Resolve: viewType:Type -> IView
    abstract member GetDescriptor: name:vname -> IViewDescriptor
    abstract member GetDescriptor: viewType:Type -> IViewDescriptor

 and [<Interface>] IView =
    abstract Publish<'M> : message:'M * [<ParamArray>] topics:string[] -> unit
    abstract member FindRegion: regionName:rname -> IRegion
    abstract ViewModel:obj

 and [<Interface>] IRegion = 
    abstract member ActivateView: name:vname -> IView
    abstract member ActivateView<'v when 'v:>IView> : unit -> 'v
    abstract Name:rname with get
    //abstract Item: string -> IView with get

type [<Interface>] ICommandModel =
    abstract member Name:string with get
    abstract member Description:string with get
    abstract member DisplayName:string with get
    abstract member Tooltip:string with get

