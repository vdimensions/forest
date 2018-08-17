namespace Forest

open System


type [<AbstractClass>] ForestNodeAttribute(name: string) =    
    inherit Attribute()
    member __.Name with get() = name

[<AttributeUsage(AttributeTargets.Class)>]
type [<Sealed>] ViewAttribute(name: string) = 
    inherit ForestNodeAttribute(name)
    member val AutowireCommands = false with get, set

[<AttributeUsage(AttributeTargets.Method)>]
type [<Sealed>] CommandAttribute(name: string) = inherit ForestNodeAttribute(name)

