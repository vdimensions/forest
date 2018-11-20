namespace Forest

open System


/// <summary>
/// An interface representing a Forest event bus
/// </summary>
type [<Interface>] IEventBus = 
    inherit IDisposable
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
    abstract member Publish<'M> : sender : IView * message : 'M * [<ParamArray>] topics : string[] -> unit
    abstract member Subscribe: subscriptionHandler : ISubscriptionHandler -> topic : string -> IEventBus
    abstract member Unsubscribe: sender : IView -> IEventBus

 and [<Interface>] ISubscriptionHandler =
    abstract member Invoke: arg : obj -> unit
    abstract MessageType: Type with get
    abstract Receiver: IView