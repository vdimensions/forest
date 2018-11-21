namespace Forest


// internal functionality needed by the forest runtime
type [<Interface>] internal IRuntimeView =
    inherit IView

    abstract member AcquireRuntime: node : TreeNode -> vd : IViewDescriptor -> runtime : IForestRuntime -> unit
    abstract member AbandonRuntime: runtime : IForestRuntime -> unit

    abstract member Load: unit -> unit
    abstract member Resume: model : obj -> unit

    abstract InstanceID : TreeNode with get
    abstract Descriptor : IViewDescriptor with get
    abstract Runtime : IForestRuntime with get

 and [<Interface>] internal IForestRuntime =
    abstract member SubscribeEvents: receiver : IRuntimeView -> unit
    abstract member UnsubscribeEvents: receiver : IRuntimeView -> unit

    abstract member GetViewModel: id : TreeNode -> obj option
    abstract member SetViewModel: silent : bool -> id : TreeNode -> model : 'T -> 'T

    abstract member ClearRegion: node : TreeNode -> region : rname -> unit
    abstract member GetRegionContents: node : TreeNode -> region : rname -> IView seq
    abstract member RemoveViewFromRegion: node : TreeNode -> region : rname -> predicate : System.Predicate<IView> -> unit

    abstract member ActivateView: view : vname * region : rname * parent : TreeNode -> IView
    abstract member ActivateView: model : 'm * view : vname * region : rname * parent : TreeNode -> IView<'m>
    abstract member ActivateAnonymousView<'v when 'v :> IView> : region : rname * parent : TreeNode -> 'v
    abstract member ActivateAnonymousView<'v, 'm when 'v :> IView<'m>> : model : 'm * region : rname * parent : TreeNode -> 'v
    abstract member ExecuteCommand: issuer : IRuntimeView -> command : cname -> arg : obj -> unit
    abstract member PublishEvent: sender : IRuntimeView -> message : 'M -> topics : string array -> unit