namespace Forest

// internal functionality needed by the forest runtime
type [<Interface>] internal IViewState =
    inherit IView

    abstract member AcquireRuntime: runtime:IForestRuntime -> unit
    abstract member AbandonRuntime: runtime:IForestRuntime -> unit

    abstract member Load: unit -> unit
    abstract member Resume: viewModel:obj -> unit

    abstract InstanceID:HierarchyKey with get, set
    abstract Descriptor:IViewDescriptor with get, set
    abstract Runtime:IForestRuntime with get

  and [<Interface>] internal IForestRuntime =
    abstract member SubscribeEvents: receiver:IViewState -> unit
    abstract member UnsubscribeEvents: receiver:IViewState -> unit
    //
    abstract member GetViewModel: id:HierarchyKey -> obj option
    abstract member SetViewModel: silent:bool -> id:HierarchyKey -> viewModel:'T -> 'T
    //
    abstract member ActivateView: parent: HierarchyKey -> region:rname -> view:vname -> IView
    abstract member ActivateAnonymousView<'v when 'v:>IView> : parent:HierarchyKey -> region:rname -> 'v
    abstract member ExecuteCommand: issuer:IViewState -> command:cname -> arg:obj -> unit
    abstract member PublishEvent: sender:IViewState -> message:'M -> topics:string array -> unit