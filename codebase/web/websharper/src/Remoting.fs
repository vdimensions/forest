namespace Forest.Web.WebSharper

open Forest
open Forest.Web.WebSharper
open WebSharper
open WebSharper.UI

type [<Sealed;NoEquality;NoComparison>] Remoting =
    [<DefaultValue>]
    static val mutable private _facade : IForestEngine voption

    [<DefaultValue>]
    static val mutable private _nodeProvider : INodeStateProvider voption

    static member internal Init (forest : IForestEngine, nodeProvider : INodeStateProvider) =
        match Remoting._facade with
        | ValueNone -> 
            Remoting._facade <- ValueSome forest
            Remoting._nodeProvider <- ValueSome nodeProvider
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
    static member ExecuteCommand cmd instanceID (arg : obj) =
        async { 
            let result =
                match (Remoting._facade, Remoting._nodeProvider) with
                | (ValueSome facade, ValueSome nodeProvider) -> 
                    facade.ExecuteCommand(cmd, instanceID, arg)
                    nodeProvider.UpdatedNodes
                | _ -> invalidOp "A forest facade has not been initialized yet"
            return result
        }