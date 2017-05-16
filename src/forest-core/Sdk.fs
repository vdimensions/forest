namespace Forest.Sdk
open System

type ForestNode =
    | IViewNode
    | IRegionNode

//[<AbstractClass>]
//type AbstractForestEngine =
//    interface IForestEngine with
//        member x.Execute ctx node =
//            ()