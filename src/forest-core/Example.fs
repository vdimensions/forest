namespace Forest
open Forest
open Forest.Dom
open System

type Example() = 
    member this.Execute (path: Path) (s: IForestState) (c: IForestContext) : unit =
        //let domIndex = c |> s.Push path 
        //let view: IView = c.Registry.Resolve (domIndex.Item[""])
        //let newDomIndex = traverse view
        let domIndex = s.DomIndex;
        let engine = DefaultForestEngine()
        let changedDomIndex = engine.Execute(c, domIndex)

        ()