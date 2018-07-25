namespace Forest
open System

type [<AbstractClass>] ForestNodeAttribute(name: string) =    
    inherit Attribute()
    member this.Name with get() = name
