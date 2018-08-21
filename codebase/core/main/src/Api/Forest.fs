namespace Forest

open System

type [<Interface>] IForestDescriptor = 
    abstract Name: string with get
    
type [<Interface>] IViewDescriptor = 
    inherit IForestDescriptor
    abstract ViewType: Type with get
    abstract ViewModelType: Type with get
    abstract Commands: Index<ICommandDescriptor, string> with get

 and [<Interface>] ICommandDescriptor = 
    inherit IForestDescriptor
    abstract ArgumentType: Type with get
    abstract member Invoke: arg: obj -> v:IView -> unit

 and [<Interface>] IViewRegistry =
    abstract member Register: t: Type -> IViewRegistry
    abstract member Register<'T when 'T:> IView> : unit -> IViewRegistry
    abstract member Resolve: name: string -> IView
    abstract member GetDescriptor: name: string -> IViewDescriptor

 and [<Interface>] IView =
    abstract Publish<'M> : message: 'M * [<ParamArray>] topics: string[] -> unit
    abstract member FindRegion: regionName: string -> IRegion
    abstract ViewModel: obj

 and [<Interface>] IRegion = 
    abstract member ActivateView: name:string -> IView
    abstract Name: string with get
    //abstract Item: string -> IView with get

