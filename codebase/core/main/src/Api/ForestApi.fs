namespace Forest

open Forest.Collections

open System
open System.Collections.Generic


type [<Interface>] IViewDescriptor = 
    abstract Name : vname with get
    abstract ViewType : Type with get
    abstract ViewModelType : Type with get
    abstract Commands : Index<ICommandDescriptor, cname> with get
    abstract Events : IEventDescriptor seq with get

 and [<Interface>] ICommandDescriptor = 
    abstract member Invoke : arg : obj -> v : IView -> unit
    abstract Name : cname with get
    abstract ArgumentType : Type with get

 and [<Interface>] IEventDescriptor =
    abstract member Trigger : view : IView -> message : obj -> unit
    abstract Topic : string with get
    abstract MessageType : Type with get

 and [<Interface>] IViewRegistry =
    abstract member Register : t : Type -> IViewRegistry
    abstract member Register<'T when 'T:>IView> : unit -> IViewRegistry
    abstract member Resolve : name : vname -> IView
    abstract member Resolve : name : vname * model:obj -> IView
    abstract member Resolve : viewType : Type -> IView
    abstract member Resolve : viewType : Type * model : obj -> IView
    abstract member GetDescriptor : name : vname -> IViewDescriptor
    abstract member GetDescriptor : viewType : Type -> IViewDescriptor

 and [<Interface>] IView =
    inherit IDisposable
    abstract Publish<'M> : message:'M * [<ParamArray>] topics:string[] -> unit
    abstract member FindRegion : regionName : rname -> IRegion
    abstract member Close : unit -> unit
    abstract ViewModel : obj

 and [<Interface>] IView<'T> =
    inherit IView
    abstract ViewModel : 'T with get, set

 and [<Interface>] IRegion = 
    abstract member ActivateView : name : vname -> IView
    abstract member ActivateView<'m> : name : vname * model : 'm -> IView<'m>
    abstract member ActivateView<'v when 'v :> IView> : unit -> 'v
    abstract member ActivateView<'v, 'm when 'v :> IView<'m>> : model : 'm -> 'v
    abstract member Clear : unit -> IRegion
    abstract member Remove : System.Predicate<IView> -> IRegion
    abstract Name : rname with get
    abstract Views : IView seq with get
    //abstract Item: vname -> IView with get
