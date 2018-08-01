namespace Forest

open Forest.NullHandling

open System
open System.Collections.Generic


[<AttributeUsage(AttributeTargets.Class)>]
type [<Sealed>] ViewAttribute(name: string) = 
    inherit ForestNodeAttribute(name)
    member val AutowireCommands = false with get, set


type [<Interface>] IViewDescriptor = 
    abstract Name: string with get
    abstract ViewType: Type with get
    abstract ViewModelType: Type with get
    abstract Commands: IEnumerable<ICommandDescriptor> with get

