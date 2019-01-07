namespace Forest.Web.WebSharper

open System.Collections.Generic
open Forest
open Forest.UI
open WebSharper
open WebSharper.UI
open Axle.Web.AspNetCore.Session
open Microsoft.AspNetCore.Http

type Node =
    {
        Hash: thash;
        Name: vname;
        Model: obj;
        Regions: array<rname*thash array>
    }

type [<Interface>] IDocumentRenderer =
    abstract member Doc: unit -> Doc

type [<Interface>] INodeStateProvider =
    abstract member Nodes: Node array with get

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

    static member Facade 
        with get() = 
            match Remoting._facade with
            | ValueSome f -> f
            | ValueNone -> invalidOp "A forest facade has not been initialized yet"

    [<Rpc>]
    static member GetNodes () : Async<Node array> = 
        async {
            let nodes =
                match Remoting._nodeProvider with
                | ValueSome p -> p.Nodes
                | ValueNone -> Array.empty
            return nodes
        }
    [<Rpc>]
    static member ExecuteCommand hash cmd (arg : obj) : unit = 
        Remoting.Facade.ExecuteCommand hash cmd arg |> ignore