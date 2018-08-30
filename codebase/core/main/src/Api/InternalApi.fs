namespace Forest

// internal functionality needed by the forest runtime
type [<Interface>] internal IRuntimeView =
    inherit IView

    abstract member AcquireRuntime: runtime:IForestRuntime -> unit
    abstract member AbandonRuntime: runtime:IForestRuntime -> unit

    abstract member Load: unit -> unit
    abstract member Resume: viewModel:obj -> unit

    abstract InstanceID:TreeNode with get, set
    abstract Descriptor:IViewDescriptor with get, set
    abstract Runtime:IForestRuntime with get

  and [<Interface>] internal IForestRuntime =
    abstract member SubscribeEvents: receiver:IRuntimeView -> unit
    abstract member UnsubscribeEvents: receiver:IRuntimeView -> unit
    //
    abstract member GetViewModel: id:TreeNode -> obj option
    abstract member SetViewModel: silent:bool -> id:TreeNode -> viewModel:'T -> 'T
    //
    abstract member ActivateView: parent: TreeNode -> region:rname -> view:vname -> IView
    abstract member ActivateAnonymousView<'v when 'v:>IView> : parent:TreeNode -> region:rname -> 'v
    abstract member ExecuteCommand: issuer:IRuntimeView -> command:cname -> arg:obj -> unit
    abstract member PublishEvent: sender:IRuntimeView -> message:'M -> topics:string array -> unit