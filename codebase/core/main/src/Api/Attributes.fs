namespace Forest

open System
open Axle.Verification


type [<AbstractClass;NoComparison>] ForestNodeAttribute(name : string) =    
    inherit Attribute()
    do ignore <| ``|NotNull|`` "name" name
    member __.Name with get() = name

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)>]
type [<Sealed;NoComparison>] ViewAttribute(name : string) = inherit ForestNodeAttribute(name)

[<AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)>]
type [<Sealed;NoComparison>] CommandAttribute(name : string) = inherit ForestNodeAttribute(name)

[<AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)>]
type [<Sealed;NoComparison>] SubscriptionAttribute(topic : string) = 
    inherit Attribute()
    do ignore <| ``|NotNull|`` "topic" topic
    member __.Topic with get() = topic

