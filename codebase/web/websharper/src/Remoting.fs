namespace Forest.Web.WebSharper

open Forest
open Forest.Web.WebSharper
open WebSharper
open WebSharper.UI

type [<Sealed;NoEquality;NoComparison>] Remoting =
    [<DefaultValue>]
    static val mutable private _facade : IForestFacade voption

    [<DefaultValue>]
    static val mutable private _nodeProvider : INodeStateProvider voption

    static member internal Init (forest : IForestFacade) =
        match Remoting._facade with
        | ValueNone -> 
            Remoting._facade <- ValueSome forest
            Remoting._nodeProvider <- (forest :?> INodeStateProvider) |> ValueSome
        | ValueSome _ -> invalidOp "A forest facade is already initialized"

    [<Rpc>]
    static member GetNodes() : Async<Node array> =
        async {
            let nodes =
                match Remoting._nodeProvider with
                | ValueSome p -> p.AllNodes
                | ValueNone -> Array.empty
            return nodes
        }
    [<Rpc>]
    static member ExecuteCommand cmd hash (arg : obj) =
        async { 
            let result =
                match (Remoting._facade, Remoting._nodeProvider) with
                | (ValueSome facade, ValueSome nodeProvider) -> 
                    facade.ExecuteCommand cmd hash arg |> ignore 
                    nodeProvider.UpdatedNodes
                | _ -> invalidOp "A forest facade has not been initialized yet"
            return result
        }