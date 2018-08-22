namespace Forest

// internal functionality needed by the forest engine
type [<Interface>] internal IViewInternal =
    inherit IView

    abstract member Load: unit -> unit

    abstract EventBus: IEventBus with get, set
    abstract InstanceID: Identifier with get, set
    abstract Descriptor: IViewDescriptor with get, set
    abstract ViewStateModifier: IViewStateModifier with get, set

  and [<Interface>] internal IViewStateModifier =
    abstract member GetViewModel: id: Identifier -> obj option
    abstract member SetViewModel: silent: bool -> id: Identifier -> viewModel: 'T -> 'T
    abstract member SubscribeEvents: v: IViewInternal -> unit
    abstract member UnsubscribeEvents: v: IViewInternal -> unit
    abstract member ActivateView: parent: Identifier -> region: string -> name: string -> IView