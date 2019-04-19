namespace Forest


// internal functionality needed by the forest runtime
type [<Interface>] internal IExecView =
    inherit IView

    abstract member AcquireRuntime : node : TreeNode -> vd : IViewDescriptor -> runtime : IForestExecutionContext -> unit
    abstract member AbandonRuntime : runtime : IForestExecutionContext -> unit

    abstract member Load : unit -> unit
    abstract member Resume : model : obj -> unit

    abstract InstanceID : TreeNode with get
    abstract Descriptor : IViewDescriptor with get
    abstract Context : IForestExecutionContext with get

 and [<Interface>] internal IForestExecutionContext =
    abstract member SubscribeEvents : receiver : IExecView -> unit
    abstract member UnsubscribeEvents : receiver : IExecView -> unit

    abstract member GetViewModel : id : TreeNode -> obj option
    abstract member SetViewModel : silent : bool -> id : TreeNode -> model : 'T -> 'T

    abstract member ClearRegion : node : TreeNode -> region : rname -> unit
    abstract member GetRegionContents : node : TreeNode -> region : rname -> IView seq
    abstract member RemoveViewFromRegion : node : TreeNode -> region : rname -> predicate : System.Predicate<IView> -> unit

    abstract member ActivateView : viewHandle : ViewHandle * region : rname * parent : TreeNode -> IView
    abstract member ActivateView : viewHandle : ViewHandle * region : rname * parent : TreeNode * model : obj -> IView
    abstract member ExecuteCommand : command : cname -> issuer : IExecView -> arg : obj -> unit
    abstract member PublishEvent : sender : IExecView -> message : 'M -> topics : string array -> unit