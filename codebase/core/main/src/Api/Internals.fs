namespace Forest

open System


// internal functionality needed by the forest engine
type [<Interface>] internal IViewInternal =
    inherit IView
    ///// <summary>
    ///// Submits the current view state to the specified <see cref="IForestContext"/> instance.
    ///// </summary>
    ///// <param name="context">
    ///// The <see cref="IForestRuntime" /> instance to manage the state of the current view.
    ///// </param>
    //abstract member Submit: ctx: IForestContext -> unit

    abstract member ResumeState: obj -> unit

    abstract EventBus: IEventBus with get, set
    abstract InstanceID: Identifier with get, set
    abstract ViewStateModifier: IViewStateModifier with get, set

  and [<Interface>] internal IViewStateModifier =
    abstract member GetViewModel: id: Identifier -> obj option
    abstract member SetViewModel: silent: bool -> id: Identifier -> viewModel: obj -> unit
    abstract member ActivateView: parent: Identifier -> region: string -> name: string -> IView