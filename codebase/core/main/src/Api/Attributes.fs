namespace Forest

open Forest.NullHandling

open System


type [<AbstractClass>] ForestNodeAttribute(name: string) =    
    inherit Attribute()
    do ignore <| isNotNull "name" name
    member __.Name with get() = name

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)>]
type [<Sealed>] ViewAttribute(name: string) = inherit ForestNodeAttribute(name)

[<AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)>]
type [<Sealed>] CommandAttribute(name: string) = inherit ForestNodeAttribute(name)

[<AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)>]
type [<Sealed>] SubscriptionAttribute(topic: string) = 
    inherit Attribute()
    do ignore <| isNotNull "topic" topic
    member __.Topic with get() = topic

