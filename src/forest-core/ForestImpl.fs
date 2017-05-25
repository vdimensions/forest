namespace Forest
open System
open System.Reflection
open System.Collections.Generic
open Forest.Dom


type DefaultForestRuntime() as self =
    let container = DefaultContainer()
    let viewRegistry = ViewRegistry(container)
    member this.Registry with get(): IViewRegistry = upcast viewRegistry
    interface IForestRuntime with
        member this.Registry = self.Registry