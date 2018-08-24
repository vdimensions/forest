namespace Forest

// internal functionality needed by the forest engine
type [<Interface>] internal IViewState =
    inherit IView

    abstract member EnterModificationScope: modifier:IViewStateModifier -> unit
    abstract member LeaveModificationScope: modifier:IViewStateModifier -> unit
    abstract member Load: unit -> unit

    abstract InstanceID: HierarchyKey with get, set
    abstract Descriptor: IViewDescriptor with get, set
    abstract ViewStateModifier: IViewStateModifier with get

  and [<Interface>] internal IViewStateModifier =
    abstract member GetViewModel: id:HierarchyKey -> obj option
    abstract member SetViewModel: silent:bool -> id:HierarchyKey -> viewModel:'T -> 'T
    abstract member PublishEvent: sender:IView * message:'M * [<System.ParamArray>]topics:string array -> unit
    abstract member SubscribeEvents: receiver:IViewState -> unit
    abstract member UnsubscribeEvents: receiver:IViewState -> unit
    abstract member ActivateView: parent:HierarchyKey -> region:string -> view:string -> IView