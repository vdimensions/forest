namespace Forest

// internal functionality needed by the forest engine
type [<Interface>] internal IViewState =
    inherit IView

    abstract member EnterModificationScope: modifier: IViewStateModifier -> unit
    abstract member LeaveModificationScope: modifier: IViewStateModifier -> unit
    abstract member Load: unit -> unit

    abstract EventBus: IEventBus with get, set
    abstract InstanceID: Identifier with get, set
    abstract Descriptor: IViewDescriptor with get, set
    abstract ViewStateModifier: IViewStateModifier with get

  and [<Interface>] internal IViewStateModifier =
    abstract member GetViewModel: id: Identifier -> obj option
    abstract member SetViewModel: silent: bool -> id: Identifier -> viewModel: 'T -> 'T
    abstract member SubscribeEvents: v: IViewState -> unit
    abstract member UnsubscribeEvents: v: IViewState -> unit
    abstract member ActivateView: parent: Identifier -> region: string -> name: string -> IView