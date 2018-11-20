namespace Forest

open Forest.NullHandling

open System


type [<AbstractClass;NoComparison>] ForestNodeAttribute(name : string) =    
    inherit Attribute()
    do ignore <| isNotNull "name" name
    member __.Name with get() = name

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)>]
type [<Sealed;NoComparison>] ViewAttribute(name : string) = inherit ForestNodeAttribute(name)

[<AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)>]
type [<Sealed;NoComparison>] CommandAttribute(name : string) = inherit ForestNodeAttribute(name)

[<AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)>]
type [<Sealed;NoComparison>] SubscriptionAttribute(topic : string) = 
    inherit Attribute()
    do ignore <| isNotNull "topic" topic
    member __.Topic with get() = topic

