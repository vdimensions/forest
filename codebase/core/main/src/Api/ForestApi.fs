namespace Forest

open Forest.Collections
open System


type [<Interface>] IViewDescriptor = 
    abstract Name : vname with get
    abstract ViewType : Type with get
    abstract ModelType : Type with get
    abstract Commands : Index<ICommandDescriptor, cname> with get
    abstract Events : IEventDescriptor seq with get
    abstract IsSystemView : bool

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
    abstract member Register<'T when 'T :> IView> : unit -> IViewRegistry
    abstract member Resolve : descriptor : IViewDescriptor -> IView
    abstract member Resolve : descriptor : IViewDescriptor * model : obj -> IView
    abstract member GetDescriptor : name : vname -> IViewDescriptor
    abstract member GetDescriptor : viewType : Type -> IViewDescriptor

/// <summary>
/// An interface representing the concept of a logical view. 
/// A logical view encompasses the data to be displayed to the end-user (the model); and the possible user interactions (commands) allowed.
/// </summary>
and [<Interface>] IView =
    inherit IDisposable
    abstract Publish<'M> : message : 'M * [<ParamArray>] topics : string[] -> unit
    abstract member FindRegion : name : rname -> IRegion
    abstract member Close : unit -> unit
    abstract Model : obj

and [<Interface>] IView<'T> =
    inherit IView
    abstract member UpdateModel : Func<'T, 'T> -> unit
    abstract Model : 'T with get

and [<Interface>] IRegion = 
    abstract member ActivateView : name : vname -> IView
    abstract member ActivateView<'m> : name : vname * model : 'm -> IView<'m>
    abstract member ActivateView<'v when 'v :> IView> : unit -> 'v
    abstract member ActivateView<'v, 'm when 'v :> IView<'m>> : model : 'm -> 'v
    abstract member Clear : unit -> IRegion
    abstract member Remove : System.Predicate<IView> -> IRegion
    abstract Name : rname with get
    abstract Views : IView seq with get

module ViewRegistry =
    let register<'T when 'T :> IView> (reg : IViewRegistry) =
        reg.Register<'T>()
    let registerViewType (viewType : Type) (reg : IViewRegistry) =
        reg.Register(viewType)

    let internal getDescriptorByName (name : vname) (reg : IViewRegistry) = 
        reg.GetDescriptor name

    let internal getDescriptorByType (viewType : Type) (reg : IViewRegistry) = 
        reg.GetDescriptor viewType

    let internal getDescriptor (viewHandle : ViewHandle) =
        match viewHandle with
        | ByName n -> getDescriptorByName n
        | ByType t -> getDescriptorByType t

    let internal resolve (descriptor : IViewDescriptor) (model : obj option) (reg : IViewRegistry) =
        match model with
        | None -> reg.Resolve descriptor
        | Some m -> reg.Resolve (descriptor, m)

/// An interface representing a system view, that is a special type of view which
/// aids the internal workings of Forest, rather than serving any presentational purpose.
/// System views are never being rendered.
type ISystemView = interface inherit IView end
