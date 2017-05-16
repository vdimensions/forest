namespace Forest.Sdk
open System
open Forest

type ForestNode =
    | IViewNode
    | IRegionNode

[<AbstractClass>]
type AbstractForestEngine =
    interface IForestEngine with
        member x.Execute ctx node =
            ()