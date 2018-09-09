namespace Forest


type [<Interface>] IViewFactory = 
    abstract member Resolve: vm:IViewDescriptor -> IView

type [<Interface>] IView<'T> =
    inherit IView
    abstract ViewModel:'T with get, set