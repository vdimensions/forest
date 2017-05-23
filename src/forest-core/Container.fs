namespace Forest


[<Interface>]
type IContainer = 
    abstract member Resolve: vm: View.Metadata -> IView
