namespace Forest
open System

[<AbstractClass>]
type ForestNodeAttribute(name: string) =    
    inherit Attribute()
    member this.Name with get() = name
