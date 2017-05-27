namespace Forest
open Forest.Dom

type [<Interface>] IForestState = 
    abstract member Push: path: Path -> context: IForestContext -> IDomIndex

type Example() = 
    member this.Execute (path: Path) (s: IForestState) (c: IForestContext) : unit =
        //let domIndex = c |> s.Push path 
        //let view: IView = c.Registry.Resolve (domIndex.Item[""])
        //let newDomIndex = traverse view

        ()