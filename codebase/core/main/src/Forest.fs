namespace Forest

open Forest.Dom

open System;
open System.Collections.Generic


type [<Interface>] IViewRegistry =
    abstract member Register: t: Type -> IViewRegistry
    abstract member Register<'T when 'T:> IView> : unit -> IViewRegistry
    abstract member Resolve: viewNode: IViewNode -> IView
    abstract member Resolve: name: string -> IView
    abstract member GetViewMetadata: name: string -> IViewDescriptor option
and [<Interface>] IView =
    abstract Publish<'M> : message: 'M * [<ParamArray>] topics: string[] -> unit
    abstract Regions: IIndex<IRegion, string> with get
    abstract ViewModel: obj
and [<Interface>] IRegion = 
    abstract Views: IIndex<IView, string> with get 
    abstract Name: string with get
and [<Interface>] IViewState = 
    abstract member SuspendState: Path*obj -> unit 
    abstract member SuspendState: v:IView -> unit
    abstract member ResumeState: path: Path -> obj
and [<Interface>] IForestContext =
    abstract Registry: IViewRegistry with get
and [<Interface>] IViewFactory = 
    abstract member Resolve: vm: IViewDescriptor -> IView
/// <summary>
/// An interface representing a Forest event bus
/// </summary>
and [<Interface>] IEventBus = 
    /// <summary>
    /// Publishes a message trough the event bus
    /// </summary>
    /// <typeparam name="T">
    /// The type of the message being pushed.
    /// </typeparam>
    /// <param name="sender">
    /// The <see cref="IView">view</see> instance that is sending the message.
    /// </param>
    /// <param name="message">
    /// The object representing the message.
    /// </param>
    /// <param name="topics">
    /// A list of topics names referring to the topics the message will be sent to.
    /// <para>
    /// Use of empty string value <c>""</c> will cause the message to be received by all subscribers for the 
    /// given message type that did not specify topic name during subscription.
    /// </para>
    /// <para>
    /// Use of empty list will cause the message to be broadcast to all subscribers regardless of their subscription topic.
    /// </para>
    /// </param>
    /// <returns>
    /// A <see cref="bool">boolean</see> value indicating whether the message has been sent to a valid topic.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sender"/> is <c>null</c>
    /// </exception>
    abstract member Publish<'M> : sender: IView * message: 'M * [<ParamArray>] topics: string[] -> unit;
    abstract member Subscribe: subscriptionHandler: ISubscriptionHandler -> topic: string -> IEventBus
    abstract member Unsubscribe : sender: IView -> IEventBus
and [<Interface>] ISubscriptionHandler =
    abstract member Invoke: arg: obj -> unit;
    abstract MessageType: Type with get
    abstract Receiver: IView with get

type [<Interface>] IForestEngine =
    abstract member CreateDomIndex: ctx: IForestContext -> data: obj -> IDomIndex
    abstract member Execute: ctx: IForestContext -> node: IViewNode -> IView

type [<Interface>] internal IForestContextAware =
    abstract member InitializeContext: ctx: IForestContext -> unit

[<Flags>]
type internal ViewChange =
    | ViewModel // of something
