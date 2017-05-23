namespace Forest
open System


[<Interface>]
type IContainer = 
    abstract member Resolve: vm: View.Metadata -> IView
