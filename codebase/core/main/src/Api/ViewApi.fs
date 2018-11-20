namespace Forest


type [<Interface>] IViewFactory = 
    abstract member Resolve: vd : IViewDescriptor -> IView
    abstract member Resolve: vd : IViewDescriptor * model : obj -> IView